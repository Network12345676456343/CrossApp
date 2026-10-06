using Core.Domain;
using Core.Dto;

namespace Core.Import;

public sealed record DomainImportResult<TDomain, TDto>(
    IReadOnlyList<TDomain> ValidEntities,
    IReadOnlyList<string> ParsingErrors,
    IReadOnlyList<(TDto Dto, string ErrorMessage)> InvariantErrors);

public static class DomainImportExtensions
{
    public static DomainImportResult<BookCopy, BookCopyDto> ToDomainEntities(
        this ImportResult<BookCopyDto> importResult)
    {
        var valid = new List<BookCopy>();
        var invariantErrors = new List<(BookCopyDto, string)>();

        foreach (var dto in importResult.Items)
        {
            try
            {
                var entity = BookCopy.FromDto(dto);
                valid.Add(entity);
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                invariantErrors.Add((dto, ex.Message));
            }
        }

        return new DomainImportResult<BookCopy, BookCopyDto>(
            valid.AsReadOnly(),
            importResult.Errors,
            invariantErrors.AsReadOnly());
    }
}