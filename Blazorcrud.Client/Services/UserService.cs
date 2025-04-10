using Blazorcrud.Client.Shared;
using Blazorcrud.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace Blazorcrud.Client.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpService _httpService;
        private readonly ILocalStorageService _localStorageService;

        private readonly NavigationManager _navigationManager;
        private readonly string _userKey = "user";
        private readonly string _tokenKey = "authToken";

        public User? User { get; private set; }

        public UserService(IHttpService httpService, ILocalStorageService localStorageService, NavigationManager navigationManager)
        {
            _httpService = httpService;
            _localStorageService = localStorageService;
            _navigationManager = navigationManager;
        }

        public async Task Initialize()
        {
            // Récupérer les utilisateurs
            var users = await _localStorageService.GetItem<List<Register>>("users") ?? new List<Register>();

            // Ajouter admin si absent
            if (!users.Any(u => u.Username == "admin"))
            {
                users.Add(new Register
                {
                    FirstName = "Super",
                    LastName = "Admin",
                    Username = "admin",
                    Password = "admin",
                    Role = "admin"
                });

                await _localStorageService.SetItem("users", users);
            }

            // Ne pas connecter automatiquement l'utilisateur ici
            // Supprimer toute connexion persistée par erreur
            await _localStorageService.RemoveItem(_userKey);
            await _localStorageService.RemoveItem(_tokenKey);

            User = null;
        }

        public async Task Login(Login model)
        {
            var users = await _localStorageService.GetItem<List<Register>>("users") ?? new List<Register>();

            var user = users.FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);

            if (user == null)
            {
                throw new Exception("Identifiants incorrects.");
            }

            User = new User
            {
                Username = user.Username,
                FirstName = user.FirstName,
                Role = user.Role
            };

            await _localStorageService.SetItem(_userKey, User);

            // Faux token d’authentification
            var token = Guid.NewGuid().ToString();
            await _localStorageService.SetItem(_tokenKey, token);
        }

        public async Task Register(Register model)
        {
            var users = await _localStorageService.GetItem<List<Register>>("users") ?? new List<Register>();

            if (users.Any(u => u.Username == model.Username))
            {
                throw new Exception("Ce compte est déjà utilisé.");
            }

            model.Role ??= "employee"; // Rôle par défaut

            users.Add(model);
            await _localStorageService.SetItem("users", users);
        }

        public async Task Logout()
        {
            User = null;
            await _localStorageService.RemoveItem(_userKey);
            await _localStorageService.RemoveItem(_tokenKey);
            _navigationManager.NavigateTo("/user/login");
        }

        public async Task<bool> IsAuthenticated()
        {
            var token = await _localStorageService.GetItem<string>(_tokenKey);
            return !string.IsNullOrEmpty(token);
        }

        public async Task<bool> IsAdmin()
        {
            var user = await _localStorageService.GetItem<User>(_userKey);
            return user != null && user.Role == "admin";
        }

        // Facultatif : méthode pour réinitialiser tout le local storage (utile en dev)
        public async Task ResetAll()
        {
            await _localStorageService.RemoveItem("users");
            await _localStorageService.RemoveItem(_userKey);
            await _localStorageService.RemoveItem(_tokenKey);
            User = null;
        }
    }
}
