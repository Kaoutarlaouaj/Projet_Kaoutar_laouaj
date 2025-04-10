using Blazorcrud.Client.Shared;
using Blazorcrud.Shared.Data;
using Blazorcrud.Shared.Models;
using Microsoft.AspNetCore.Components;

namespace Blazorcrud.Client.Services
{
    public class PersonService : IPersonService
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly NavigationManager _navigationManager;
        private readonly string _personKey = "persons";
        private readonly IHttpService _httpService;  // Ajoutez cette dépendance pour le service HTTP

        public List<Person> Persons { get; private set; } = new List<Person>();

        public PersonService(ILocalStorageService localStorageService, NavigationManager navigationManager, IHttpService httpService)
        {
            _localStorageService = localStorageService;
            _navigationManager = navigationManager;
            _httpService = httpService;
        }

      

        public async Task AddPerson(Person newPerson)
        {
            var persons = await _localStorageService.GetItem<List<Person>>(_personKey) ?? new List<Person>();

            // Ajoute la nouvelle personne
            persons.Add(newPerson);
            await _localStorageService.SetItem(_personKey, persons);
        }

        public async Task UpdatePerson(Person updatedPerson)
        {
            var persons = await _localStorageService.GetItem<List<Person>>(_personKey) ?? new List<Person>();

            // Trouver et mettre à jour la personne
            var index = persons.FindIndex(p => p.PersonId == updatedPerson.PersonId);
            if (index != -1)
            {
                persons[index] = updatedPerson;
                await _localStorageService.SetItem(_personKey, persons);
            }
        }

        public async Task DeletePerson(int personId)
        {
            var persons = await _localStorageService.GetItem<List<Person>>(_personKey) ?? new List<Person>();

            // Supprimer la personne par son ID
            var personToRemove = persons.FirstOrDefault(p => p.PersonId == personId);
            if (personToRemove != null)
            {
                persons.Remove(personToRemove);
                await _localStorageService.SetItem(_personKey, persons);
            }
        }

        // Facultatif : méthode pour réinitialiser tout le local storage (utile en dev)
        public async Task ResetAll()
        {
            await _localStorageService.RemoveItem(_personKey);
            Persons.Clear();
        }

        // Méthode pour obtenir une liste paginée de personnes par nom depuis le localStorage
        public async Task<PagedResult<Person>> GetPeople(string name, string page)
        {
            var persons = await _localStorageService.GetItem<List<Person>>(_personKey) ?? new List<Person>();

            // Prévention contre null
            name = name ?? string.Empty;

            // Filtrer les personnes
            var filteredPersons = persons.Where(p =>
                (!string.IsNullOrEmpty(p.FirstName) && p.FirstName.Contains(name, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(p.LastName) && p.LastName.Contains(name, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            // Pagination
            int pageNumber = int.TryParse(page, out int result) ? result : 1;
            int pageSize = 5;
            var pagedPersons = filteredPersons.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResult<Person>
            {
                Results = pagedPersons,
                RowCount = filteredPersons.Count,
                PageSize = pageSize,
                CurrentPage = pageNumber,
                PageCount = (int)Math.Ceiling((double)filteredPersons.Count / pageSize)
            };
        }




        // Méthode pour obtenir une personne spécifique par son ID depuis le localStorage
        public async Task<Person> GetPerson(int id)
        {
            var persons = await _localStorageService.GetItem<List<Person>>(_personKey) ?? new List<Person>();
            return persons.FirstOrDefault(p => p.PersonId == id);
        }
    }
}
