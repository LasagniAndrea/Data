// Pour debugguer pas à pas ce module mettre un point d'arrêt sur chacune des 2 méthodes suivantes:
//      ---------------------------------------------------------------------------------------------------------------------
//      class FeeProcessing
//          Cette classe contient entre autre:
//          - FeeRequest: objet contenant les éléments sources (Trade, Parties, Otherparties, ...)
//          - FeeResponse[]: objet contenant les éléments résultats (EventCode, Payment, ...)
//          - FeeMatrixs/FeeMatrix: objet contenant les données de la table FEEMATRIX
//      method Calc()
//          Cette méthode lit la table FEEMATRIX pour y extraire les différents record matchant avec le trade.
//          Ensuite, pour chaque record retenu, elle construit un élément FeeResponse
//          NB: Les records non retenus voit la propriété "feeMatrix.CommentForDebug" alimentée pour expliquer le rejet.
//              De même la fenêtre de sortie de VS détail le rejet.
//      ---------------------------------------------------------------------------------------------------------------------
//
//      ---------------------------------------------------------------------------------------------------------------------
//      class FeeResponse
//          Cette classe contient entre autre:
//          - EventCode: type de payment ADP/OPP
//          - Payment: objet classique contenant les caractéristiques du payment, avec toutefois une alimentation de SpheresSource
//          - ErrorMessage/InfoMessage: Message d'erreur en cas d'anomalie ou d'information en cas de succès
//      method Calc()
//          Cette méthode calcule via un élément FeeMatrix (Date, Bracket, Formula, Min/Max, ...)
//          les différentes valeurs nécessaires à la constitution de l'objet Payment (Payer/Receiver, Montant, ...)
//          NB: En cas d'échec, l'objet Payment est mis à null et la propriété "feeMatrix.CommentForDebug" est alimentée pour expliquer l'échec.
//      ---------------------------------------------------------------------------------------------------------------------