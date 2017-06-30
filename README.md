# AdminAPI 构架说明
日期: 06-30-2017，版本: v1.0.0

### 概述
AdminAPI 是一个使用长轮询机制实现信息推送的交互式平台，结合传统 MVC 模型与后端 JSON 格式 API 进行无刷新实时推送。本项目在 OSX 环境下构设完成，并参考微软在近年对使用 Mac 系统开发 .NET 项目的最新支持与指导。开发环境为微软在2015年04月新发布的 VisualStudioCode ([软件主页](https://code.visualstudio.com/))（[.NET开发具体环境构架](https://code.visualstudio.com/docs/other/dotnet))，并使用其内置 NuGet 程序包管理系统引入 .NetCore 实现本地部署。项目数据库使用微软公司 ASP.NET 提供的 EntityFrameworkCore ([资料](https://docs.microsoft.com/en-us/ef/core/index)) 以简化数据库接入代码。

平台默认群体推送，基本功能包括接收与发送群体信息和动作，并广播登录登出情况。附加功能包括指定式静音功能以屏蔽指定用户信息。平台带有验证系统，需登录注册后即可使用上述功能。

### 后端
平台后端在初始时生成一个名为 `Globals.pushStore` 的全局变量，用于内存储全局推送信息，屏蔽情况和其他用户细节。
```c#
public class Store{
        // 内置变量

        // 用于存储用户发送过的信息。使用哈希表用户名(钥)对应字符串列(值)的格式。
        private Hashtable _store;
        
        // 用于存储是否要向用户推送信息的布尔值。在存储用户信息时，此哈希表内值会被改写为真，并触发长轮询。
        private Hashtable _shouldUpdateStore;
        
        // 用于存储全部在线用户的用户名。
        private List<string> _userIds;
        
        // 用于记忆上次向用户推送的终止位置，防止重复推送。
        private Hashtable _nextStartStore;
        
        // 用于存储全部注册过用户的用户名。
        private List<string> _allUserIds;
        
        // 用于存储主动屏蔽方的屏蔽对象。
        private Hashtable _silenced;
        
        // 用于存储被动屏蔽方的屏蔽源对象。
        private Hashtable _beingSilenced;

        // 公共方法

        // 用于在用户注册时，将其用户名与全局变量进行捆绑，以接收与传递信息。
        // 参量： userId - 用户名
        public void addSubscription(string userId);

        // 用于在用户登出时，取消对其的推送。
        // 参量： userId - 用户名
        public void removeSubscription(string userId);

        // 用于记载新的用户动态信息并推送至其他用户。
        // 参量： userId - 用户名， newInfo - 新信息
        public void addInfo(string userId, string newInfo);

        // 用于问询是否需要对用户进行新一轮推送。
        // 参量： userId - 用户名
        public bool shouldUpdate(string userId);

        // 用于问询用户是否在线。
        // 参量： userId - 用户名
        public bool isSubscribed(string userId);

        // 用于生成针对源用户的系统环境（包括接收与屏蔽细节）
        // 参量： userId - 源用户名
        public List<string> userStatus(string userId);

        // 用于屏蔽用户
        // 参量： userId - 主动屏蔽方用户名, targetUserId - 被动屏蔽方用户名
        public void silence(string userId, string targetUserId);

        // 用于解除屏蔽用户
        // 参量： userId - 主动解除屏蔽方用户名, targetUserId - 被动解除屏蔽方用户名
        public void unsilence(string userId, string targetUserId);
    }
```

后端