namespace BookShop
{
    using Data;
    using Initializer;
    using System;
    using System.Linq;
    using BookShop.Common.Extentions;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.EntityFrameworkCore;
    using System.Globalization;

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
                //Console.WriteLine(GetBooksNotReleasedIn(db, 2000));
                //Console.WriteLine(GetBooksByCategory(db, new string[] { "horror", "mystery", "drama" }));
                //Console.WriteLine(GetBooksReleasedBefore(db, "30-12-1989"));
                //Console.WriteLine(GetAuthorNamesEndingIn(db, "dy"));
                Console.WriteLine(GetBookTitlesContaining(db, "WOR"));
                Console.ReadKey();
            }
        }

        //1. Age Restriction
        //Return in a single string all book titles, each on a new line, that have age restriction, equal to the given command. Order the titles alphabetically.
        public static string GetBooksByAgeRestriction(BookShopContext context, string command)
        {
            var result = context.Books.Where(x => x.AgeRestriction.ToString().ToLower() == command.ToLower())
                                      .Select(x => x.Title)
                                      .OrderBy(x => x).ToList();

            return result.JoinWithNewLine();
        }

        //2. Golden Books
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

        //3. Books by Price
        //Return in a single string all titles and prices of books with price higher than 40, each on a new row in the format given below.Order them by price descending
        //O Pioneers! - $49.90
        public static string GetBooksByPrice(BookShopContext context)
        {
            var result = context.Books.Where(book => book.Price > 40)
                                        //.Select(x => new { x.Title, x.Price })
                                        .OrderByDescending(x => x.Price)
                                        .Select(x => new { x.Title, x.Price }).ToList();

            return result.BuildStringFromDTO(currencySymbol: "$", separator: " - ");
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

        //5. Book Titles by Category
        //Return in a single string the titles of books by a given list of categories.The list of categories will be given in a single line separated with one or more spaces.Ignore casing.Order by title alphabetically.
        public static string GetBooksByCategory(BookShopContext context, string[] filter)
        {
            var result = context.Books
                                 .Include(x => x.BookCategories)
                                 .ThenInclude(x => x.Category)
                                 .Where(x => x.BookCategories
                                                .All(c => filter.Contains(c.Category.Name)))
                                 .Select(x => x.Title)
                                 .OrderBy(x => x)
                                 .ToList();

            return result.JoinWithNewLine();
        }

        //6. Released Before Date
        //Return the title, edition type and price of all books that are released before a given date.The date will be a string in format dd-MM-yyyy.Return all of the rows in a single string, ordered by release date descending.
        //Input -> 12-04-1992 Output -> If I Forget Thee Jerusalem - Gold - $33.21
        public static string GetBooksReleasedBefore(BookShopContext context, string dateString)
        {
            //var provider = CultureInfo.InvariantCulture; var dateString = "12-04-1992"; var format = "dd-MM-yyyy";
            var parsedDatetime = DateTime.ParseExact(dateString, "dd-MM-yyyy", CultureInfo.InvariantCulture);

            var result = context.Books.Where(x => x.ReleaseDate < parsedDatetime)
                                      .OrderByDescending(x => x.ReleaseDate)
                                      .Select(x => new { x.Title, x.EditionType, x.Price })
                                      .ToList();

            return result.BuildStringFromDTO(currencySymbol: "$", separator: " - ");
        }

        //7. Author Search
        //Return the full names of authors, whose first name ends with a given string
        //e -> George Powell, Jane Ortiz; dy -> Randy Morales
        public static string GetAuthorNamesEndingIn(BookShopContext context, string input)
        {
            var result = context.Books
                                .Include(x => x.Author)
                                .Where(x => x.Author.FirstName.EndsWith(input))
                                .Select(x => new { x.Author.FirstName, x.Author.LastName })
                                .Distinct()
                                .ToDictionary(k => new { k.FirstName, k.LastName }, v => true);

            return result.Keys.BuildStringFromDTO();
        }

        //8. Book Search
        //Return the titles of book, which contain a given string. Ignore casing. Return all titles in a single string, each on a new row, ordered alphabetically.
        //WOR -> Great Work of Time, Terrible Swift Sword
        public static string GetBookTitlesContaining(BookShopContext context, string input)
        {
            var result = context.Books.Where(x => x.Title.ToLower().Contains(input))
                                        .Select(x => x.Title)
                                        .OrderBy(x => x)
                                        .ToList();
            return result.JoinWithNewLine();
        }

    }
}
