using System.ComponentModel.DataAnnotations;
using MongoDB.Bson.Serialization.Attributes;

namespace MiddleOffice.Models;

/// <summary>
/// Représente l'affectation d'un validateur à une demande de validation.
/// Chaque validateur peut approuver ou rejeter avec un commentaire.
/// </summary>
public class ValidatorAssignment
{
    [BsonElement("id")]
    [Required(ErrorMessage = "L'identifiant du validateur est obligatoire")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    // Le rôle détermine qui peut effectuer cette validation (ex: quality_manager, content_editor)
    [BsonElement("role")]
    [Required(ErrorMessage = "Le rôle du validateur est obligatoire")]
    public string Role { get; set; }

    // Si on veut assigner un utilisateur précis plutôt qu'un rôle générique
    [BsonElement("userId")]
    public string? UserId { get; set; }

    // Nom affiché du validateur pour l'interface
    [BsonElement("displayName")]
    public string? DisplayName { get; set; }

    // Statut de cette validation : pending, approved, rejected
    [BsonElement("status")]
    public string Status { get; set; } = "pending";

    // Commentaire laissé par le validateur (surtout utile en cas de rejet)
    [BsonElement("comment")]
    public string? Comment { get; set; }

    // Date à laquelle la décision a été prise
    [BsonElement("decidedAt")]
    public DateTime? DecidedAt { get; set; }

    // Identifiant de l'utilisateur qui a pris la décision
    [BsonElement("decidedBy")]
    public string? DecidedBy { get; set; }

    // Ordre de validation (pour les validations séquentielles)
    [BsonElement("order")]
    public int Order { get; set; } = 1;

    // Indique si cette validation est bloquante
    [BsonElement("mandatory")]
    public bool Mandatory { get; set; } = true;
}
