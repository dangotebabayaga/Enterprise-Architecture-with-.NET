using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DemoEditor.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MiddleOffice.Models;

public class Template
{
    [BsonId()]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonIgnore]
    public ObjectId TechnicalId { get; set; }

    [BsonElement("entityId")]
    [Required(ErrorMessage = "Business identifier of a template is mandatory")]
    public string EntityId { get; set; }

    [BsonElement("title")]
    public List<InternationalizedString> Title { get; set; } = new List<InternationalizedString>();

    [BsonElement("fields")]
    public List<Field> Fields { get; set; } = new List<Field>();

    [BsonElement("possibleDecisions")]
    public List<Decision> PossibleDecisions { get; set; } = new List<Decision>();

    [BsonElement("status")]
    public string Status { get; set; } = "active";

    // Liste des validateurs requis pour ce template (validation multi-acteurs)
    [BsonElement("requiredValidators")]
    public List<ValidatorRequirement> RequiredValidators { get; set; } = new();
}

public class Field
{
    [BsonElement("id")]
    [Required(ErrorMessage = "Field id is mandatory")]
    public string Id { get; set; }

    [BsonElement("name")]
    public List<InternationalizedString> Name { get; set; } = new List<InternationalizedString>();

    [BsonElement("inputType")]
    public string InputType { get; set; }

    [BsonElement("required")]
    public bool Required { get; set; } = true;

    [BsonElement("visible")]
    public bool Visible { get; set; } = true;
}

/// <summary>
/// Définit un validateur requis dans un template.
/// Utilisé pour configurer les rôles qui doivent approuver une demande.
/// </summary>
public class ValidatorRequirement
{
    // Rôle Keycloak requis pour effectuer cette validation
    [BsonElement("role")]
    [Required(ErrorMessage = "Le rôle du validateur est obligatoire")]
    public string Role { get; set; }

    // Titre affiché dans l'interface (multilingue)
    [BsonElement("title")]
    public List<InternationalizedString> Title { get; set; } = new();

    // Ordre de validation (1 = premier, 2 = après, etc.)
    // Les validateurs du même ordre peuvent valider en parallèle
    [BsonElement("order")]
    public int Order { get; set; } = 1;

    // Si true, cette validation est obligatoire pour approuver la demande
    [BsonElement("mandatory")]
    public bool Mandatory { get; set; } = true;
}