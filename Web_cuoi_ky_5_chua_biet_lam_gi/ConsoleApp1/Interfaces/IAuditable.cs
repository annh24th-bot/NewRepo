namespace Core.Database.Interfaces
{
    public interface IAuditable
    {
        public Guid ? CreatedBy { get; set;}
        public DateTime? Createdon { get; set; }
        public Guid? ModifiedBy { get; set; }
        public DateTime? Modifieddon { get; set; }
    }
}
