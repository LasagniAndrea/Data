using System;
using System.Collections.Generic;
using System.Text;

namespace EFS.SpheresRiskPerformance.CashBalance
{


    /// <summary>
    /// Mode d’exécution du traitement de calcul des CashBalances en fonction du résultat, le plus récent, des traitements EOD relatifs.
    /// <para>Liste des valeurs autorisées pour le paramètre PARAM_CTRL_EOD_MODE</para>
    /// <para>NB: Hormis la valeur NONE, toutes les valeurs fonctionnent de paire avec le second paramètre CTRL_EOD_CSSCUSTODIANLIST.
    ///           Ce dernier est destiné à décrire un périmètre de "Clearing House" (ex. TBD).
    /// </para>
    /// </summary>
    /// FI 20141126 [20526] Add Enum
    public enum ControlEODMode
    {
        /// <summary>
        /// NONE: Aucun contrôle, le calcul du CashBalance est opéré, que celui-ci l’ait déjà été ou pas, et que les traitements EOD soient opérés ou pas. 
        /// <para> NB : Cette valeur est la valeur par défaut.</para>
        /// </summary>
        NONE,

        /// <summary>
        /// MODE1: Le calcul du CashBalance est opéré si et uniquement si les 2 conditions suivantes sont réunies :
        /// <para>Condition 1 : le résultat, le plus récent, de chaque traitement EOD relatif au périmètre des "Clearing House" est en Succès (ou Warning).</para>
        /// <para>Condition 2a: aucun calcul de CashBalance n'a été opéré</para>
        /// <para>              ou</para>
        /// <para>             le dernier calcul de CashBalance opéré est "incomplet"(*)</para>
        /// <para></para> 
        /// <para>(*) un CashBalance est considéré "incomplet" dès lors qu’au moins un des traitements EOD relatifs au périmètre des "Clearing House" n’était pas en Succès (ou Warning) lors de son calcul.</para>
        /// </summary>
        MODE1,

        /// <summary>
        /// MODE2: Le calcul du CashBalance est opéré si et uniquement si les 2 conditions suivantes sont réunies :
        /// <para>Condition 1 : le résultat, le plus récent, de chaque traitement EOD relatif au périmètre des "Clearing House" est en Succès (ou Warning).</para>
        /// <para>Condition 2b: aucun calcul de CashBalance n'a été opéré</para>
        /// <para>              ou</para>
        /// <para>             le dernier calcul de CashBalance opéré est "incomplet"(*)</para>
        /// <para>              ou</para>
        /// <para>              au moins un des traitements EOD relatifs au périmètre des "Clearing House" a été ré-exécuté postérieurement au dernier calcul de CashBalance et son résultat est en Succès (ou Warning).</para>
        /// <para></para> 
        /// <para>(*) un CashBalance est considéré "incomplet" dès lors qu’au moins un des traitements EOD relatifs au périmètre des "Clearing House" n’était pas en Succès (ou Warning) lors de son calcul.</para>
        /// </summary>
        MODE2,

        /// <summary>
        /// MODE3: Le calcul du CashBalance est opéré si et uniquement si les 2 conditions suivantes sont réunies :
        /// <para>Condition 1 : le résultat, le plus récent, de chaque traitement EOD relatif au périmètre des "Clearing House" est en Succès (ou Warning).</para>
        /// <para>Condition 2c: aucun calcul de CashBalance n'a été opéré</para>
        /// <para>              ou</para>
        /// <para>             le dernier calcul de CashBalance opéré est "incomplet"(*)</para>
        /// <para>              ou</para>
        /// <para>              tous les traitements EOD relatifs au périmètre des "Clearing House" ont été ré-exécuté postérieurement au dernier calcul de CashBalance et leur résultat est en Succès (ou Warning).</para>
        /// <para></para> 
        /// <para>(*) un CashBalance est considéré "incomplet" dès lors qu’au moins un des traitements EOD relatifs au périmètre des "Clearing House" n’était pas en Succès (ou Warning) lors de son calcul.</para>
        /// </summary>
        MODE3
    }

    /// <summary>
    /// Pilote le comportement lorsque le paramètre CTRL_EOD_MODE  a pour valeur ('MODE1','MODE2','MODE3') et que les derniers traitements de fin de jounée ne sont pas en succès (où warning) 
    /// </summary>
    /// FI 20141126 [20526] Add Enum
    public enum ControlEODLogStatus
    {
        /// <summary>
        /// Génère une information => le traitement n'est pas exécuté et termine en none (bleue dans le tracker)
        /// </summary>
        INFO,
        /// <summary>
        /// Génère un warning => le traitement n'est pas exécuté et termine en warning (orange dans le tracker)
        /// </summary>
        WARNING,
        /// <summary>
        /// Génère une erreur => le traitement n'est pas exécuté et termine en erreur (rouge dans le tracker)
        /// </summary>
        ERROR,
    }

}
