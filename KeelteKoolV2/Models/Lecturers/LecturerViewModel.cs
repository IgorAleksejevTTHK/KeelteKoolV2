namespace KeelteKoolV2.Models.Lecturers
{
    //ViewModel õpetaja andmete näitamiseks index, details ja delete vaates
    public class LecturerViewModel
    {
        public Guid? Id { get; set; } //optional sest index vaade ei vaja seda
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Qualifications { get; set; } //optional sest index vaade ei vaja seda
    }
}
