//using Microsoft.Extensions.Caching.Memory;
//using Microsoft.Extensions.DependencyInjection;

//namespace Acme.Core.Extensions;

//public static class CacheExtensions
//{
//    public static void AddCaching(this IServiceCollection serviceCollection)
//    {
//        serviceCollection.AddSingleton<IMemoryCache>();
//    }

//    public static T GetItem<T>(string key)
//    {
//        try
//        {
//            IMemoryCache cache = new MemoryCache();


//        }
//        catch (Exception exception)
//        {
//            Console.WriteLine(exception);
//            throw;
//        }
//    }
//}

