namespace AcademicManager.Domain.Entities;

public class Grado
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;     // "1er Grado", "2do Grado", "1er Año"
    public string Nivel { get; set; } = string.Empty;      // Primaria, Secundaria, Universitario
    public int Orden { get; set; }                         // Para ordenamiento
    public bool Activo { get; set; } = true;
}
