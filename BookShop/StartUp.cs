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
                //Console.WriteLine(GetBookTitlesContaining(db, "WOR"));
                //Console.WriteLine(GetBooksByAuthor(db, "po"));
                //Console.WriteLine(CountBooks(db, 12));
                //Console.WriteLine(CountCopiesByAuthor(db));
                //Console.WriteLine(GetTotalProfitByCategory(db));
                //Console.WriteLine(GetMostRecentBooks(db));
                //Console.WriteLine(IncreasePrices(db));
                Console.WriteLine(RemoveBooks(db));
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
        //9. Book Search by Author
        //Return all titles of books and their authors’ names for books, which are written by authors whose last names start with the given string.
        //Ignore casing.Order by book id ascending. R -> The Heart Is Deceitful Above All Things (Bozhidara Rysinova),  His Dark Materials(Bozhidara Rysinova), The Heart Is a Lonely Hunter(Bozhidara Rysinova)
        public static string GetBooksByAuthor(BookShopContext context, string input)
        {
            var result = context.Books.Include(x => x.Author)
                                      .Where(x => x.Author.LastName.ToLower().StartsWith(input))
                                      .OrderBy(x => x.BookId)
                                      .Select(x => Tuple.Create(x.Title, "(" + x.Author.FirstName + " " + x.Author.LastName + ")"))
                                      .ToList();
            return result.BuildStringFromDTO();
        }

        //10. Count Books
        //Return the number of books, which have a title longer than the number given as an input.
        public static int CountBooks(BookShopContext context, int lengthCheck)
        {
            var result = context.Books.Where(x => x.Title.Length > lengthCheck).Count();
            return result;
        }

        //11. Total Book Copies
        //Return the total number of book copies for each author.Order the results descending by total book copies
        public static string CountCopiesByAuthor(BookShopContext context)
        {
            var result = context.Books.Include(x => x.Author)
                                        .GroupBy(x => new { x.Author.FirstName, x.Author.LastName },
                                                    (x, y) => new
                                                    {
                                                        AuthorName = (x.FirstName + " " + x.LastName),
                                                        SumOfCopies = y.Sum(z => z.Copies)
                                                    })
                                        .OrderByDescending(x => x.SumOfCopies).ToList();

            return result.BuildStringFromDTO(separator: " - ");
        }

        //12.Profit by Category
        //Return the total profit of all books by category.Profit for a book can be calculated by multiplying its number of copies by the price per single book.
        //Order the results by descending by total profit for category and ascending by category name.
        //Art $6428917.79
        public static string GetTotalProfitByCategory(BookShopContext context)
        {
            var result = context.BooksCategories.Include(x => x.Book).Include(x => x.Category)
                                                .GroupBy(x => x.Category.Name,
                                                                (x, y) => new
                                                                {
                                                                    CategoryName = x,
                                                                    TotalProfit = y.Sum(z => z.Book.Price * z.Book.Copies)
                                                                })
                                                .OrderByDescending(x => x.TotalProfit)
                                                .ThenBy(x => x.CategoryName)
                                                .ToList();

            return result.BuildStringFromDTO(currencySymbol: "$", pricePropertyName: "TotalProfit");
        }


        //13. Most Recent Books
        // Get the most recent books by categories.The categories should be ordered by name alphabetically.
        // Only the top 3 most recent books from each category - ordered by release date (descending). 
        // Select and print the category name, and for each book – its title and release year. Output must be like:
        //--Action
        //Brandy ofthe Damned(2015)
        //Bonjour Tristesse(2013)
        //By Grand Central Station I Sat Down and Wept(2010)
        //--Adventure
        //The Cricket on the Hearth(2013)
        //Dance Dance Dance(2002)
        //Cover Her Face(2000)
        public static string GetMostRecentBooks(BookShopContext context)
        {
            var resultFromQuery = context.Categories
                                           .Include(x => x.CategoryBooks).ThenInclude(x => x.Book)
                                           .GroupBy(x => x.Name,
                                                        (x, y) => new
                                                        {
                                                            CategoryName = x,
                                                            RecentBooks = y.Select(z => z.CategoryBooks
                                                                                            .Select(a => new { a.Book.Title, a.Book.ReleaseDate })
                                                                                            .OrderByDescending(b => b.ReleaseDate).Take(3)
                                                                                   )
                                                        })
                                           .ToList();

            var finalResult = string.Empty;
            foreach (var itemOuter in resultFromQuery)
            {
                var categoryName = itemOuter.CategoryName;
                var aggregateLinesInCategory = string.Empty;

                foreach (var itemInner in itemOuter.RecentBooks)
                {
                    var linesOfBookTitleYear = itemInner.Select(x => x.Title + " (" + x.ReleaseDate.GetValueOrDefault().Year + ")");
                    aggregateLinesInCategory = linesOfBookTitleYear.Aggregate(new StringBuilder(),
                                                                        (sb, x) => sb.AppendLine(x),
                                                                        sb => sb.ToString());
                }

                finalResult += ($"--{categoryName}{Environment.NewLine}{aggregateLinesInCategory}");
            }

            return finalResult;
        }

        //14. Increase Prices
        //Increase the prices of all books released before 2010 by 5.
        private static string IncreasePrices(BookShopContext context)
        {
            var result = context.Books
                                .Where(x => x.ReleaseDate.GetValueOrDefault().Year < 2010)
                                .Select(c => new { OldPrice = c.Price, NewPrice = (c.Price + 5) })
                                .ToList();

            //return result.BuildStringFromDTO(separator: " -> ", currencySymbol: "$", pricePropertyName: "OldPrice");
            return result.BuildStringFromDTO(separator: " -> ", currencySymbol: "$", pricesProps: new string[] { "NewPrice", "OldPrice" });
        }

        //15. Remove Books
        //Remove all books, which have less than 4200 copies. Return an int - the number of books that were deleted from the database.
        //Output 34
        public static int RemoveBooks(BookShopContext context)
        {
            var itemsForDeleting = context.Books.Where(x => x.Copies < 4200);

            context.Books.RemoveRange(itemsForDeleting);
            var result = itemsForDeleting.Count();
            context.SaveChanges();

            return result;
        }
    }
}
