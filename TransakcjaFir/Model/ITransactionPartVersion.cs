namespace TransakcjaFir.Model
{
    public interface ITransactionPartVersion
    {
        long Id { get; set; }
        int VersionNumber { get; set; }
        bool IsLastVersion { get; set; }
    }
}
