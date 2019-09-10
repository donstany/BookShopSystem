namespace BookShop
{
    using Data;
    using Initializer;
    using System;
    using System.Linq;
    using BookShop.Common.Extentions;
    using System.Collections.Generic;
    using System.Text;

    public class StartUp
    {
        public static void Main()
        {
            using (var db = new BookShopContext())
            {
                DbInitializer.ResetDatabase(db);

                var getBooksByAgeRestriction = GetBooksByAgeRestriction(db, "miNor");
                //Console.WriteLine(getBooksByAgeRestriction);
                //Console.WriteLine(GetGoldenBooks(db));
                //Console.WriteLine(GetBooksByPrice(db));
                Console.WriteLine(GetBooksNotReleasedIn(db, 2000));
                Console.ReadKey();
            }
        }

        //1.Age Restriction
        //Return in a single string all book titles, each on a new line, that have age restriction, equal to the given command. Order the titles alphabetically.
        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            var result = context.Books.Where(x => x.AgeRestriction.ToString().ToLower() == command.ToLower())
                                      .Select(x => x.Title)
                                      .OrderBy(x => x).ToList();

            return result.JoinWithNewLine();
        }

        //2.Golden Books
        //Return in a single string titles of the golden edition books that have less than 5000 copies, each on a new line. Order them by book id ascending
        public static string GetGoldenBooks(BookShopContext context)
        {
            var result = context.Books.Where(book => book.Copies < 5000)
                                        //.Select(book => new { book.Title, book.BookId })
                                        .OrderBy(goldenBook => goldenBook.BookId)
                                         //.Select(goldenBook => new { goldenBook.Title, goldenBook.BookId }).ToList();
                                         .Select(goldenBook => goldenBook.Title).ToList();
            return result.JoinWithNewLine();
        }

        //3.Books by Price
        //Return in a single string all titles and prices of books with price higher than 40, each on a new row in the format given below.Order them by price descending
        //O Pioneers! - $49.90
        public static string GetBooksByPrice(BookShopContext context)
        {
            var result = context.Books.Where(book => book.Price > 40)
                                        //.Select(x => new { x.Title, x.Price })
                                        .OrderByDescending(x => x.Price)
                                        .Select(x => new { x.Title, x.Price }).ToList();

            return result.BuildStringFromDTO(currencySymbol:"$", separator:" - ");
        }

        //4. Not Released In
        //Return in a single string all titles of books that are NOT released on a given year.Order them by book id ascending.
        public static string GetBooksNotReleasedIn(BookShopContext context, int year)
        {

            var result = context.Books.Where(x => x.ReleaseDate.GetValueOrDefault().Year != year)
                                        .OrderBy(x => x.BookId)
                                        .Select(x => x.Title);

            return result.JoinWithNewLine();
        }
    }
}
