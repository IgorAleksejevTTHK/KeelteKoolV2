namespace KeelteKoolV2.Models.Lecturers
{
    //ViewModel õpetaja loomiseks ja muutmiseks
    public class LecturerCreateUpdateViewModel
    {
        public Guid? Id { get; set; } //optional sest loomisel id veel puudub
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Qualifications { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
