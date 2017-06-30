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
        private List<string> _allUserIds;
        private Hashtable _silenced;
        private Hashtable _beingSilenced;
        public Store(){
            _userIds = new List<string>();
            _shouldUpdateStore = new Hashtable();
            _store = new Hashtable();
            _nextStartStore = new Hashtable();
            _allUserIds = new List<string>();
            _silenced = new Hashtable();
            _beingSilenced = new Hashtable();
        }
        public void addSubscription(string userId){
            _userIds.Add(userId);
            _store.Add(userId, new List<string>());
            _silenced.Add(userId, new List<string>());
            _beingSilenced.Add(userId, new List<string>());
            _shouldUpdateStore.Add(userId, false);
            _nextStartStore.Add(userId, 0);
            _allUserIds.Add(userId);
        }
        public void removeSubscription(string userId){
            _userIds.Remove(userId);
            _store.Remove(userId);
            _shouldUpdateStore.Remove(userId);
            _nextStartStore.Remove(userId);
            _silenced.Remove(userId);
            _beingSilenced.Remove(userId);
        }
        public void addInfo(string userId, string newInfo){
            List<string> skipList = _beingSilenced[userId] as List<string>;
            Hashtable skipHash = new Hashtable();
            for(int i = 0; i < skipList.Count; i++){
                skipHash[skipList[i]] = true;
            }

            List<string> entry;
            for(int i = 0; i < _userIds.Count; i++){
                if(skipHash.ContainsKey(_userIds[i])){
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
        public List<string> userStatus(string userId){
            List<string> silenced = _silenced[userId] as List<string>;
            Hashtable silencedHash = new Hashtable();
            for(int i = 0; i < silenced.Count; i++){
                silencedHash[silenced[i]] = true;
            }
            Hashtable onlineHash = new Hashtable();
            for(int j = 0; j < _userIds.Count; j++){
                onlineHash[_userIds[j]] = true;
            }

            List<string> result = new List<string>();
            string name, ol, sil;
            for(int k = 0; k < _allUserIds.Count; k++){
                name = _allUserIds[k];
                ol = (bool)onlineHash.ContainsKey(name) ? "Online" : "Offline";
                sil = (bool)silencedHash.ContainsKey(name) ? "Unsilence" : "Silence";
                result.Add(string.Format("{0};{1};{2}", name, ol, sil));
            }
            return result;
        }
        public void silence(string userId, string targetUserId){
            List<string> silenceList = _silenced[userId] as List<string>;
            List<string> beingSilencedList = _beingSilenced[targetUserId] as List<string>;
            if(silenceList.IndexOf(targetUserId) < 0){
                silenceList.Add(targetUserId);
            }
            if(beingSilencedList.IndexOf(userId) < 0){
                beingSilencedList.Add(userId);
            }
        }
        public void unsilence(string userId, string targetUserId){
            List<string> silenceList = _silenced[userId] as List<string>;
            List<string> beingSilencedList = _beingSilenced[targetUserId] as List<string>;
            if(silenceList.IndexOf(targetUserId) >= 0){
                silenceList.Remove(targetUserId);
            }
            if(beingSilencedList.IndexOf(userId) >= 0){
                beingSilencedList.Remove(userId);
            }
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
            app.UseStaticFiles();
        }
    }
}
