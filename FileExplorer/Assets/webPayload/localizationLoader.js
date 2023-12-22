function loadJson() {
    return new Promise((resolve, reject) => {
        var xobj = new XMLHttpRequest();
        xobj.overrideMimeType("application/json");
        xobj.open('GET', 'http://localhost:9001/lang', true);
        xobj.onreadystatechange = function () {
            if (xobj.readyState == 4 && xobj.status == "200") {
                var data = JSON.parse(xobj.responseText);
                console.log(data);
                resolve(data);
            }
        };
        xobj.onerror = function () {
            reject(new Error("Network Error"));
        };
        xobj.send(null);
    });
}

const localizationLoader = {
    localizedStrings: "",

    async loadJsonData() {
        this.localizedStrings = await loadJson();
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
