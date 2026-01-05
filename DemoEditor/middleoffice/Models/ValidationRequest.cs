using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DemoEditor.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MiddleOffice.Models;

/// <summary>
/// Demande de validation multi-validateurs.
/// Permet de soumettre un livre à plusieurs personnes pour approbation.
/// </summary>
public class ValidationRequest
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonIgnore]
    public ObjectId TechnicalId { get; set; }

    [BsonElement("entityId")]
    [Required(ErrorMessage = "L'identifiant de la demande est obligatoire")]
    public string EntityId { get; set; } = Guid.NewGuid().ToString("N");

    // Référence vers le livre concerné par cette validation
    [BsonElement("bookId")]
    [Required(ErrorMessage = "L'identifiant du livre est obligatoire")]
    public string BookId { get; set; }

    // Titre du livre (copié pour faciliter l'affichage sans requête supplémentaire)
    [BsonElement("bookTitle")]
    public string? BookTitle { get; set; }

    // Lien vers le template utilisé pour cette validation
    [BsonElement("template")]
    public TemplateLink? Template { get; set; }

    // Statut global de la demande : pending, in_validation, approved, rejected
    [BsonElement("status")]
    public string Status { get; set; } = "pending";

    // Type de validation : all (tous doivent approuver), majority, any (un seul suffit)
    [BsonElement("validationType")]
    public string ValidationType { get; set; } = "all";

    // Liste des validateurs assignés à cette demande
    [BsonElement("validators")]
    public List<ValidatorAssignment> Validators { get; set; } = new();

    // Qui a créé cette demande
    [BsonElement("createdBy")]
    public string? CreatedBy { get; set; }

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Date de clôture (quand la validation est terminée)
    [BsonElement("completedAt")]
    public DateTime? CompletedAt { get; set; }

    // Message optionnel du demandeur
    [BsonElement("requestMessage")]
    public string? RequestMessage { get; set; }
}
