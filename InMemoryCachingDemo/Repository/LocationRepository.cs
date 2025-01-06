using InMemoryCachingDemo.Data;
using InMemoryCachingDemo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace InMemoryCachingDemo.Repository
{
    public class LocationRepository
    {
        // ApplicationDbContext instance for interacting with the database.
        private readonly ApplicationDbContext _context;

        // IMemoryCache instance for implementing in-memory caching.
        private readonly IMemoryCache _cache;

        // Constructor to initialize the ApplicationDbContext and IMemoryCache.
        public LocationRepository(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // Retrieves the list of countries, with caching.
        // Using Manual Eviction for countries with High Priority
        public async Task<List<Country>> GetCountriesAsync()
        {
            // Defines a unique key for caching the countries data.
            var cacheKey = "Countries";

            // Checks if the countries data is already cached.
            if (!_cache.TryGetValue(cacheKey, out List<Country>? countries))
            {
                // If not cached, fetches the countries list from the database asynchronously.
                countries = await _context.Countries.AsNoTracking().ToListAsync();

                // No Expiration Set
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetPriority(CacheItemPriority.High); // High priority for countries

                // Stores the fetched countries in the cache without the expiration time but with High Priority.
                _cache.Set(cacheKey, countries, cacheEntryOptions);
            }

            // Returns the cached or fetched countries, or an empty list if null.
            return countries ?? new List<Country>();
        }

        // This Method can be called after updating or deleting country data.
        public void RemoveCountriesFromCache()
        {
            var cacheKey = "Countries";
            _cache.Remove(cacheKey);
        }

        //This method is used to add a Country
        public async Task AddCountry(Country country)
        {
            _context.Countries.Add(country);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                throw;
            }

            //After adding, call the RemoveCountriesFromCache() method to clear the Country Cache
            RemoveCountriesFromCache();
        }

        // This method is used to Update a Country
        public async Task UpdateCountry(Country updatedCountry)
        {
            _context.Countries.Update(updatedCountry);
            await _context.SaveChangesAsync();

            //After updating, call the RemoveCountriesFromCache() method to clear the Country Cache
            RemoveCountriesFromCache();
        }

        // Retrieves the list of states for a specific country, with caching.
        // Using Sliding Expiration for states
        public async Task<List<State>> GetStatesAsync(int countryId)
        {
            // Defines a unique cache key based on the country ID.
            string cacheKey = $"States_{countryId}";

            // Checks if the states data for the given country ID is cached.
            if (!_cache.TryGetValue(cacheKey, out List<State>? states))
            {
                // Fetches states from the database if not cached
                states = await _context.States.Where(s => s.CountryId == countryId).AsNoTracking().ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(30)) // Setting the Sliding Expiration
                    .SetPriority(CacheItemPriority.Normal); // Normal priority for states

                // Stores the fetched states in the cache with the Sliding Expiration and Normal Priority.
                _cache.Set(cacheKey, states, cacheEntryOptions);
            }

            // Returns the cached or fetched states, or an empty list if null.
            return states ?? new List<State>();
        }

        // Retrieves the list of cities for a specific state, with caching.
        // Using Absolute Expiration for Cities
        public async Task<List<City>> GetCitiesAsync(int stateId)
        {
            // Defines a unique cache key based on the state ID.
            string cacheKey = $"Cities_{stateId}";

            // Checks if the cities data for the given state ID is cached.
            if (!_cache.TryGetValue(cacheKey, out List<City>? cities))
            {
                // Fetches cities from the database if not cached.
                cities = await _context.Cities.Where(c => c.StateId == stateId).AsNoTracking().ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(30)) // Set the Absolute Expiration
                    .SetPriority(CacheItemPriority.Low); // Low priority for cities

                // Stores the fetched cities in the cache with the Absolute Expiration and Low priority.
                _cache.Set(cacheKey, cities, cacheEntryOptions);
            }

            // Returns the cached or fetched cities, or an empty list if null.
            return cities ?? new List<City>();
        }
    }
}