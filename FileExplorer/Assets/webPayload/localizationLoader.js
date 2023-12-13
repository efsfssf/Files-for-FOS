const localizationLoader = {
  localizedStrings: "",

  loadJsonData() {
    return fetch('languages/en-us.json')
      .then(response => {
        if (!response.ok) {
          throw new Error(`Failed to fetch: ${response.status} ${response.statusText}`);
        }
        return response.json();
      })
      .then(data => {
        this.localizedStrings = data;
      })
      .catch(error => {
        console.error('Error fetching/parsing JSON:', error.message);
      });
  },

  getLocalizedString(key, index) {
    if (!this.localizedStrings) {
      // ?
    }

    return this.localizedStrings[key] && this.localizedStrings[key][index]
      ? this.localizedStrings[key][index]
      : `${index}.`; // :(
  }
};

localizationLoader.loadJsonData();
