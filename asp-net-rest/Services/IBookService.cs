using AspNetRest.Contracts;

namespace AspNetRest.Services;

public interface IBookService
{
    BookPageResponse GetPage(BookPageQuery query);

    BookResponse? Get(int id);

    bool Exists(int id);

    BookResponse Create(CreateBookRequest request);

    BookResponse? Replace(int id, UpdateBookRequest request);

    BookResponse? Patch(int id, PatchBookRequest request);

    bool Delete(int id);
}
