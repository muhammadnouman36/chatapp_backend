CREATE PROCEDURE SP_GetAllContacts
(
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @Search NVARCHAR(250) = NULL,
    @Status NVARCHAR(100) = NULL,
    @FromDate DATETIME = NULL,
    @ToDate DATETIME = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        Name,
        Email,
        Subject,
        Message,
        Status,
        CreatedAt,
        COUNT(*) OVER() AS TotalRecords
    FROM ContactUs
    WHERE
        (@Search IS NULL OR Name LIKE '%' + @Search + '%' OR Email LIKE '%' + @Search + '%' OR Subject LIKE '%' + @Search + '%')
        AND (@Status IS NULL OR Status = @Status)
        AND (@FromDate IS NULL OR CreatedAt >= @FromDate)
        AND (@ToDate IS NULL OR CreatedAt < DATEADD(DAY, 1, @ToDate))
    ORDER BY CreatedAt DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END