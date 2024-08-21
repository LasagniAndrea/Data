// Extension de la fonction de validation ASP.NET avec
// ajout d'un CSS propriétaire si contrôle invalide
var originalValidatorUpdateDisplay = null;
function extendedValidatorUpdateDisplay(obj) {
    // Appelle la méthode originale    
    if (typeof originalValidatorUpdateDisplay === "function") {
        originalValidatorUpdateDisplay(obj);
    }
    // Récupère l'état du control (valide ou invalide) et ajoute ou enlève la classe has-error    
    var control = document.getElementById(obj.controltovalidate);
    if (control) {
        var isValid = true;
        for (var i = 0; i < control.Validators.length; i += 1) {
            var vtr = control.Validators[i];
            if (false == control.Validators[i].isvalid) {
                // Alimentation du tooltip si le message d'erreur du validator est tronqué (CSS : overflow|ellipsis)
                $(vtr).removeAttr("title");
                $(vtr).removeAttr("date-original-title");
                if ($(vtr).is(":truncated")) {
                    $(vtr).attr("title", $(vtr).text());
                    $(vtr).tooltip({
                        title: this.title,
                        html: true,
                        //placement: "auto left",
                        trigger: "hover",
                        container: "#tipcontainer"
                    });
                }
                isValid = false;
                break;
            }
        }

        // Setting ou Unsetting de la classe "has-error" sur l'élément parent du contrôle
        var ancestor = $(control).parent("div");
        if (isValid) {
            $(ancestor).removeClass("has-error");
            if ($(ancestor).hasClass("input-group")) {
                $(ancestor).parent("div").removeClass("has-error");
            }
        } else {
            $(ancestor).addClass("has-error");
            if ($(ancestor).hasClass("input-group")) {
                $(ancestor).parent("div").addClass("has-error");
            }
        }
    }
}


// Réinitialisation des validateurs non Valid (isValid = false) sur Focus d'un champ Texte
// Permet de réactiver l'autocomplete
function RemoveHasErrorClass(event) {

    var control = event.target;
    if (control && (control.type === "text")) {
        if (null != control.Validators) {
            var isValid = true;
            for (var i = 0; i < control.Validators.length; i += 1) {
                var val = control.Validators[i];
                if (false == val.isvalid) {
                    isValid = false;
                    if (typeof (val.display) == "string") {
                        if (val.display == "None") {
                            break;
                        }
                        if (val.display == "Dynamic") {
                            val.style.display = "none";
                            break;
                        }
                    }
                    val.style.visibility = "hidden";
                    break;
                }
            }
            if (false == isValid) {
                // Setting ou Unsetting de la classe "has-error" sur l'élément parent du contrôle
                var ancestor = $(control).parent("div");
                $(ancestor).removeClass("has-error");
                if ($(ancestor).hasClass("input-group")) {
                    $(ancestor).parent("div").removeClass("has-error");
                }
            }
        }
    }
}

document.addEventListener("focus", RemoveHasErrorClass, true);

// Remplace la méthode ValidatorUpdateDisplay   
originalValidatorUpdateDisplay = window.ValidatorUpdateDisplay;
window.ValidatorUpdateDisplay = extendedValidatorUpdateDisplay;



