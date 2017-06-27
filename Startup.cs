using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AdminAPI.Models;

namespace AdminAPI
{
    public class Store{
        private Hashtable _store;
        private Hashtable _shouldUpdateStore;
        private List<string> _userIds;
        private Hashtable _nextStartStore;
        public Store(){
            _userIds = new List<string>();
            _shouldUpdateStore = new Hashtable();
            _store = new Hashtable();
            _nextStartStore = new Hashtable();
        }
        public void addSubscription(string userId){
            _userIds.Add(userId);
            _store.Add(userId, new List<string>());
            _shouldUpdateStore.Add(userId, false);
            _nextStartStore.Add(userId, 0);
        }
        public void removeSubscription(string userId){
            _userIds.RemoveAt(_userIds.IndexOf(userId));
            _store.Remove(userId);
            _shouldUpdateStore.Remove(userId);
            _nextStartStore.Remove(userId);
        }
        public void addInfo(string userId, string newInfo){
            List<string> entry;
            for(int i = 0; i < _userIds.Count; i++){
                if(userId == _userIds[i]){
                    continue;
                }
                entry = _store[_userIds[i]] as List<string>;
                entry.Add(newInfo);
                _shouldUpdateStore[_userIds[i]] = true;
            }
        }

        public bool shouldUpdate(string userId){
            return (bool)_shouldUpdateStore[userId];
        }
        public List<string> newInfo(string userId){
            List<string> entry = _store[userId] as List<string>;
            _shouldUpdateStore[userId] = false;

            List<string> result = new List<string>();
            for(int i = (int)_nextStartStore[userId]; i < entry.Count; i++){
                result.Add(entry[i]);
            }

            _nextStartStore[userId] = entry.Count;
            return result;
        }
        public bool isSubscribed(string userId){
            return _store.ContainsKey(userId);
        }
    }

    public static class Globals{
        public static Store pushStore = new Store();
    }
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<PostContext>(opt => opt.UseInMemoryDatabase());
            services.AddDbContext<UserContext>(opt => opt.UseInMemoryDatabase());
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
        }
    }
}
