using MongoDB.Driver;
using MiddleOffice.Models;

namespace MiddleOffice.Services;

/// <summary>
/// Service gérant la logique de validation multi-acteurs.
/// Gère la création des demandes, les décisions individuelles et le calcul du statut global.
/// </summary>
public class ValidationService
{
    private readonly IMongoDatabase _database;

    public ValidationService(IMongoDatabase database)
    {
        _database = database;
    }

    // Récupère la collection des demandes de validation
    private IMongoCollection<ValidationRequest> Requests
        => _database.GetCollection<ValidationRequest>("validation-requests");

    // Récupère la collection des templates
    private IMongoCollection<Template> Templates
        => _database.GetCollection<Template>("templates");

    /// <summary>
    /// Crée une nouvelle demande de validation pour un livre.
    /// Initialise automatiquement les validateurs à partir du template.
    /// </summary>
    public async Task<ValidationRequest> CreateValidationRequest(
        string templateId,
        string bookId,
        string bookTitle,
        string createdBy,
        string? message = null)
    {
        // On récupère le template pour connaître les validateurs requis
        var template = await Templates
            .Find(t => t.EntityId == templateId && t.Status == "active")
            .FirstOrDefaultAsync();

        if (template == null)
            throw new InvalidOperationException($"Template '{templateId}' introuvable ou inactif");

        // Création de la demande avec les validateurs du template
        var request = new ValidationRequest
        {
            BookId = bookId,
            BookTitle = bookTitle,
            CreatedBy = createdBy,
            RequestMessage = message,
            Status = "pending",
            ValidationType = "all", // Par défaut, tout le monde doit approuver
            Template = new TemplateLink
            {
                Rel = "template",
                Href = $"/templates/{templateId}",
                Title = template.Title.FirstOrDefault()?.Value ?? templateId,
                FullEntity = template
            }
        };

        // On crée un validateur pour chaque rôle requis dans le template
        foreach (var requirement in template.RequiredValidators)
        {
            request.Validators.Add(new ValidatorAssignment
            {
                Role = requirement.Role,
                DisplayName = requirement.Title.FirstOrDefault()?.Value ?? requirement.Role,
                Order = requirement.Order,
                Mandatory = requirement.Mandatory,
                Status = "pending"
            });
        }

        await Requests.InsertOneAsync(request);
        return request;
    }

    /// <summary>
    /// Récupère une demande de validation par son identifiant.
    /// </summary>
    public async Task<ValidationRequest?> GetValidationRequest(string requestId)
    {
        return await Requests
            .Find(r => r.EntityId == requestId)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Récupère toutes les demandes en attente pour un rôle donné.
    /// Utile pour afficher "mes validations à traiter".
    /// </summary>
    public async Task<List<ValidationRequest>> GetPendingRequestsForRole(string role)
    {
        return await Requests
            .Find(r => r.Status == "pending" &&
                       r.Validators.Any(v => v.Role == role && v.Status == "pending"))
            .ToListAsync();
    }

    /// <summary>
    /// Enregistre la décision d'un validateur (approve ou reject).
    /// Recalcule ensuite le statut global de la demande.
    /// </summary>
    public async Task<ValidationRequest> RecordDecision(
        string requestId,
        string validatorId,
        string decision, // "approved" ou "rejected"
        string decidedBy,
        string? comment = null)
    {
        var request = await GetValidationRequest(requestId);
        if (request == null)
            throw new InvalidOperationException($"Demande '{requestId}' introuvable");

        if (request.Status != "pending")
            throw new InvalidOperationException("Cette demande est déjà clôturée");

        // On trouve le validateur concerné
        var validator = request.Validators.FirstOrDefault(v => v.Id == validatorId);
        if (validator == null)
            throw new InvalidOperationException($"Validateur '{validatorId}' introuvable dans cette demande");

        if (validator.Status != "pending")
            throw new InvalidOperationException("Ce validateur a déjà donné sa décision");

        // On enregistre la décision
        validator.Status = decision;
        validator.Comment = comment;
        validator.DecidedAt = DateTime.UtcNow;
        validator.DecidedBy = decidedBy;

        // Mise à jour du statut global
        request.Status = CalculateGlobalStatus(request);

        if (request.Status == "approved" || request.Status == "rejected")
            request.CompletedAt = DateTime.UtcNow;

        // Sauvegarde en base
        await Requests.ReplaceOneAsync(r => r.EntityId == requestId, request);

        return request;
    }

    /// <summary>
    /// Calcule le statut global de la demande selon le type de validation.
    /// </summary>
    private string CalculateGlobalStatus(ValidationRequest request)
    {
        var mandatoryValidators = request.Validators.Where(v => v.Mandatory).ToList();

        switch (request.ValidationType)
        {
            case "all":
                // Tous les validateurs obligatoires doivent approuver
                if (mandatoryValidators.Any(v => v.Status == "rejected"))
                    return "rejected";
                if (mandatoryValidators.All(v => v.Status == "approved"))
                    return "approved";
                return "pending";

            case "majority":
                // La majorité doit approuver
                var approved = mandatoryValidators.Count(v => v.Status == "approved");
                var rejected = mandatoryValidators.Count(v => v.Status == "rejected");
                var total = mandatoryValidators.Count;

                if (approved > total / 2)
                    return "approved";
                if (rejected > total / 2)
                    return "rejected";
                return "pending";

            case "any":
                // Un seul validateur suffit
                if (mandatoryValidators.Any(v => v.Status == "approved"))
                    return "approved";
                if (mandatoryValidators.All(v => v.Status == "rejected"))
                    return "rejected";
                return "pending";

            default:
                return "pending";
        }
    }

    /// <summary>
    /// Récupère les demandes de validation pour un livre donné.
    /// </summary>
    public async Task<List<ValidationRequest>> GetRequestsForBook(string bookId)
    {
        return await Requests
            .Find(r => r.BookId == bookId)
            .SortByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
}
