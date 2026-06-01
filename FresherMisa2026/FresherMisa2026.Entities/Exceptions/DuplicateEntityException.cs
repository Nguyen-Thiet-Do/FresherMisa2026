namespace FresherMisa2026.Entities.Exceptions
{
    public class DuplicateEntityException : Exception
    {
        public string ColumnName { get; }

        public DuplicateEntityException(string message, string columnName = "") : base(message)
        {
            ColumnName = columnName;
        }
    }
}
