using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.GUI.Interface;
using EFS.Spheres.DataContracts;
using EFS.SpheresRiskPerformance.CommunicationObjects;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using EFS.SpheresRiskPerformance.DataContracts;
using EfsML.Business;
using EfsML.Enum;
using FpML.Interface;
using FpML.v44.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Class static de tools pour le calcul IMSM
    /// </summary>
    internal static class IMSMTools
    {
        #region Methods
        /// <summary>
        /// Calcule la moyenne des valeurs du dictionnaire en paramètre
        /// </summary>
        /// <param name="pExposure"></param>
        /// <returns></returns>
        public static decimal? Mean(Dictionary<DateTime, decimal> pExposure)
        {
            decimal? mean = default;
            if ((pExposure != default(Dictionary<DateTime, decimal>)) && (pExposure.Count != 0))
            {
                mean = pExposure.Average(e => e.Value);
            }
            return mean;
        }

        /// <summary>
        /// Donne la date immédiatement supérieure à une date parmis en ensemble de dates
        /// </summary>
        /// <param name="pDays">Ensemble de date</param>
        /// <param name="pDate">Date pour laquelle rechercher la date immédiatement plus récente</param>
        /// <returns></returns>
        public static DateTime GetNextDate(IEnumerable<DateTime> pDays, DateTime pDate)
        {
            DateTime nextDate = DateTime.MaxValue;
            if (pDays != default(IEnumerable<DateTime>))
            {
                IEnumerable<DateTime> supDate = pDays.Where(d => d > pDate);
                if (supDate.Count() > 0)
                {
                    nextDate = supDate.Min();
                }
            }
            return nextDate;
        }

        /// <summary>
        ///  Donne la date immédiatement inférieure à une date parmis en ensemble de dates
        /// </summary>
        /// <param name="pDays">Ensemble de date</param>
        /// <param name="pDate">Date pour laquelle rechercher la date immédiatement plus ancienne</param>
        /// <returns></returns>
        public static DateTime GetPreviousDate(IEnumerable<DateTime> pDays, DateTime pDate)
        {
            DateTime previousDate = DateTime.MinValue;
            if (pDays != default(IEnumerable<DateTime>))
            {
                IEnumerable<DateTime> infDate = pDays.Where(d => d < pDate);
                if (infDate.Count() > 0)
                {
                    previousDate = infDate.Max();
                }
            }
            return previousDate;
        }

        /// <summary>
        /// Donne la Nième date immédiatement inférieure à une date parmis en ensemble de dates
        /// </summary>
        /// <param name="pDays">Ensemble trié de date</param>
        /// <param name="pDate">Date pour laquelle rechercher la Nième date immédiatement plus ancienne</param>
        /// <param name="pNbDate">Numéro d'ordre de la date antérieure à rechercher</param>
        /// <returns></returns>
        public static DateTime GetLessNDate(IEnumerable<DateTime> pDays, DateTime pDate, int pNbDate)
        {
            DateTime lessDate = DateTime.MinValue;
            // Vérification des paramètres
            if ((pDays != default(IEnumerable<DateTime>)) && (pNbDate >= 0))
            {
                // Si le numéro d'ordre est égal à 0, retourner la date elle-même
                if (0 == pNbDate)
                {
                    lessDate = pDate;
                }
                else
                {
                    // Prendre toutes les dates de l'ensemble qui sont plus ancienne
                    var orderedDate = pDays.Where(d => d < pDate).OrderByDescending(e => e);
                    //
                    if (orderedDate.Count() > pNbDate)
                    {
                        // Prendre la Nième date la plus éloignée
                        lessDate = orderedDate.ElementAt(pNbDate - 1);
                    }
                    else if (orderedDate.Count() > 0)
                    {
                        // Il n'y a pas assez de date dans l'ensemble, retourner la plus éloignée
                        lessDate = orderedDate.Min();
                    }
                }
            }
            return lessDate;
        }
        #endregion Methods
    }

    /// <summary>
    /// Class des données statistiques (Exposure et T0Exposure) utiles au calcul
    /// </summary>
    internal class IMSMExposure
    {
        #region Members
        private readonly DateTime m_IMSMDate;
        private readonly DateTime m_IMSMPreviousDate;
        private readonly DateTime m_WindowDateMin;
        private readonly List<DateTime> m_WindowDays = new List<DateTime>();
        private readonly List<DateTime> m_ExposureDays = new List<DateTime>();
        private readonly Dictionary<DateTime, decimal> m_Exposure = new Dictionary<DateTime, decimal>();
        private decimal m_T0Exposure;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Date de calcul de l'IMSM
        /// </summary>
        public DateTime IMSMDate
        { get { return m_IMSMDate; } }

        /// <summary>
        /// Date veille du calcul de l'IMSM
        /// </summary>
        public DateTime IMSMPreviousDate
        { get { return m_IMSMPreviousDate; } }

        /// <summary>
        /// Borne inférieure de la fenêtre des données statistiques de calcul (Exposure)
        /// </summary>
        public DateTime WindowDateMin
        { get { return m_WindowDateMin; } }

        /// <summary>
        /// Ensemble des jours compris dans la fenêtre de calcul (trié du plus proche au plus éloigné)
        /// </summary>
        public List<DateTime> WindowDays
        { get { return m_WindowDays; } }

        /// <summary>
        /// Ensemble des jours pour lesquels une Exposure est présente
        /// </summary>
        public List<DateTime> ExposureDays
        { get { return m_ExposureDays; } }

        /// <summary>
        /// Ensemble des "Exposure" compris dans la fenêtre de calcul
        /// </summary>
        public Dictionary<DateTime, decimal> WindowExposure
        { get { return m_Exposure; } }

        /// <summary>
        /// "T0Exposure"
        /// </summary>
        public decimal T0Exposure
        { get { return m_T0Exposure; } }

        /// <summary>
        /// Date la plus vieille pour laquelle il existe une Exposure
        /// </summary>
        public DateTime OlderExposureDate
        {
            get
            {
                DateTime olderDate = m_IMSMDate;
                if (m_Exposure.Count > 0)
                {
                    DateTime olderExposureDate = m_Exposure.Keys.Min();
                    if (olderExposureDate < olderDate)
                    {
                        olderDate = olderExposureDate;
                    }
                }
                return olderDate;
            }
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// Construit également l'ensemble des dates de la fenêtre de calcul
        /// </summary>
        /// <param name="pDtIMSM">Date à laquelle le montant de déposit est réclamé</param>
        /// <param name="pIMSMParameters">Paramètres généraux de calcul du déposit</param>
        public IMSMExposure(DateTime pDtIMSM, decimal pT0Exposure, IMSMParameter pIMSMParameters)
        {
            m_IMSMDate = pDtIMSM;
            m_WindowDays = new List<DateTime>();
            m_Exposure = new Dictionary<DateTime, decimal>();
            m_T0Exposure = pT0Exposure;
            //
            // Construire l'ensemble des dates de la fenêtre de calcul
            DateTime currentDate = pDtIMSM;
            if (pIMSMParameters != default(IMSMParameter))
            {
                int nbDate = 0;
                while (nbDate <= pIMSMParameters.WindowSizeStatistic)
                {
                    DayOfWeek dow = currentDate.DayOfWeek;
                    if (DayOfWeek.Saturday == dow)
                    {
                        currentDate = currentDate.AddDays(-1);
                    }
                    else if (DayOfWeek.Sunday == dow)
                    {
                        currentDate = currentDate.AddDays(-2);
                    }
                    m_WindowDays.Add(currentDate);
                    currentDate = currentDate.AddDays(-1);
                    nbDate += 1;
                }
            }
            // Recherche de la date veille à la date de calcul
            m_IMSMPreviousDate = GetPreviousDate(m_IMSMDate);
            // Calcul de la date étant la borne inférieure de la fenêtre de calcul
            m_WindowDateMin = m_WindowDays.Min();
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Ajoute ou met à jour la valeur d'Exposure à une date dans un ensemble d'Exposure
        /// </summary>
        /// <param name="pValues"></param>
        /// <param name="pDate"></param>
        /// <param name="pExposure"></param>
        /// <returns>true si la valeur a été ajoutée ou mise à jour</returns>
        private bool Add(Dictionary<DateTime, decimal> pValues, DateTime pDate, decimal pExposure)
        {
            bool isAdded = false;
            // Si la date est plus récente ou égale à la plus vielle date de la fenêtre de calcul
            if (pDate >= m_WindowDateMin)
            {
                if (pValues.ContainsKey(pDate))
                {
                    pValues[pDate] = pExposure;
                }
                else
                {
                    pValues.Add(pDate, pExposure);
                }
                if (false == m_ExposureDays.Contains(pDate))
                {
                    m_ExposureDays.Add(pDate);
                }
                isAdded = true;
            }
            return isAdded;
        }

        /// <summary>
        /// Ajoute ou met à jour la valeur d'Exposure à une date
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pExposure"></param>
        /// <returns>true si la valeur a été ajoutée ou mise à jour</returns>
        public bool Add(DateTime pDate, decimal pExposure)
        {
            return Add(m_Exposure, pDate, pExposure);
        }

        /// <summary>
        /// Ajoute ou met à jour les valeurs d'Exposure
        /// </summary>
        /// <param name="pExposure">Ensemble de données d'exposur à ajouter/mettre à jour</param>
        public void Add(IEnumerable<KeyValuePair<DateTime, decimal>> pExposure)
        {
            if (pExposure != default(IEnumerable<KeyValuePair<DateTime, decimal>>))
            {
                foreach (KeyValuePair<DateTime, decimal> value in pExposure)
                {
                    Add(m_Exposure, value.Key, value.Value);
                }
            }
        }

        /// <summary>
        /// Lit l'Exposure à une date
        /// </summary>
        /// <param name="pDate"></param>
        /// <returns></returns>
        public decimal GetExposure(DateTime pDate)
        {
            m_Exposure.TryGetValue(pDate, out decimal exposure);
            return exposure;
        }

        /// <summary>
        /// Obtient la date suivante de la fenêtre de calcul
        /// </summary>
        /// <param name="pDate"></param>
        /// <returns></returns>
        public DateTime GetNextDate(DateTime pDate)
        {
            return IMSMTools.GetNextDate(m_WindowDays, pDate);
        }

        /// <summary>
        /// Obtient la date précédante de la fenêtre de calcul
        /// </summary>
        /// <param name="pDate"></param>
        /// <returns></returns>
        public DateTime GetPreviousDate(DateTime pDate)
        {
            return IMSMTools.GetPreviousDate(m_WindowDays, pDate);
        }

        /// <summary>
        /// Fournit l'ensemble des Exposures pour une sous fenêtre de calcul
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pWindowsSize"></param>
        /// <returns></returns>
        public Dictionary<DateTime, decimal> ExposureShortWindow(DateTime pDate, int pWindowsSize)
        {
            DateTime minDate = IMSMTools.GetLessNDate(m_WindowDays, pDate, pWindowsSize);
            return m_Exposure.Where(e => e.Key < pDate && e.Key > minDate).ToDictionary(e => e.Key, e => e.Value);
        }

        /// <summary>
        /// Supprime toutes les valeurs d'Exposure
        /// </summary>
        public void Clear()
        {
            m_Exposure.Clear();
        }
        #endregion Methods

        #region Test Methods
        /// <summary>
        /// Initialise les Exposure et T0Exposure avec les données de Test
        /// </summary>
        public void SetTestExposure()
        {
            #region Adding Exposure
            AddTest(new DateTime(2016, 12, 01), 69628, 69628);
            AddTest(new DateTime(2016, 11, 30), 213673, 77906);
            AddTest(new DateTime(2016, 11, 29), 477074, 234642);
            AddTest(new DateTime(2016, 11, 28), 487465, 86429);
            AddTest(new DateTime(2016, 11, 25), 252574, 103294);
            AddTest(new DateTime(2016, 11, 24), 227210, 60016);
            AddTest(new DateTime(2016, 11, 23), 207807, 97134);
            AddTest(new DateTime(2016, 11, 22), 453114, 194680);
            AddTest(new DateTime(2016, 11, 21), 450454, 97120);
            AddTest(new DateTime(2016, 11, 18), 223391, 101498);
            AddTest(new DateTime(2016, 11, 17), 247234, 108306);
            AddTest(new DateTime(2016, 11, 16), 358148, 212885);
            AddTest(new DateTime(2016, 11, 15), 655363, 297790);
            AddTest(new DateTime(2016, 11, 14), 518928, 94059);
            AddTest(new DateTime(2016, 11, 11), 195302, 70290);
            AddTest(new DateTime(2016, 11, 10), 199239, 98088);
            AddTest(new DateTime(2016, 11, 09), 213032, 445333);
            AddTest(new DateTime(2016, 11, 08), 547967, 436129);
            AddTest(new DateTime(2016, 11, 07), 545396, 89395);
            AddTest(new DateTime(2016, 11, 04), 292741, 151549);
            AddTest(new DateTime(2016, 11, 03), 320016, 125662);
            AddTest(new DateTime(2016, 11, 02), 274609, 98408);
            AddTest(new DateTime(2016, 11, 01), 550831, 324165);
            AddTest(new DateTime(2016, 10, 31), 549404, 100905);
            AddTest(new DateTime(2016, 10, 28), 219668, 95765);
            AddTest(new DateTime(2016, 10, 27), 194582, 72287);
            AddTest(new DateTime(2016, 10, 26), 219829, 92825);
            AddTest(new DateTime(2016, 10, 25), 488488, 286594);
            AddTest(new DateTime(2016, 10, 24), 494798, 106003);
            AddTest(new DateTime(2016, 10, 21), 196669, 61173);
            AddTest(new DateTime(2016, 10, 20), 183541, 91110);
            AddTest(new DateTime(2016, 10, 19), 244614, 114271);
            AddTest(new DateTime(2016, 10, 18), 449033, 260974);
            AddTest(new DateTime(2016, 10, 17), 456121, 119978);
            AddTest(new DateTime(2016, 10, 14), 209745, 76857);
            AddTest(new DateTime(2016, 10, 13), 143692, 63718);
            AddTest(new DateTime(2016, 10, 12), 209843, 118970);
            AddTest(new DateTime(2016, 10, 11), 541539, 305584);
            AddTest(new DateTime(2016, 10, 10), 568399, 163668);
            AddTest(new DateTime(2016, 10, 07), 223908, 42622);
            AddTest(new DateTime(2016, 10, 06), 49645, 49645);
            AddTest(new DateTime(2016, 10, 05), 633468, 633468);
            AddTest(new DateTime(2016, 10, 04), 619406, 619406);
            AddTest(new DateTime(2016, 10, 03), 254248, 254248);
            AddTest(new DateTime(2016, 09, 30), 296880, 296880);
            AddTest(new DateTime(2016, 09, 29), 632547, 632547);
            AddTest(new DateTime(2016, 09, 28), 608121, 608121);
            AddTest(new DateTime(2016, 09, 27), 239314, 239314);
            AddTest(new DateTime(2016, 09, 26), 239071, 239071);
            AddTest(new DateTime(2016, 09, 23), 218769, 218769);
            AddTest(new DateTime(2016, 09, 22), 493536, 493536);
            AddTest(new DateTime(2016, 09, 21), 669123, 669123);
            AddTest(new DateTime(2016, 09, 20), 424879, 424879);
            AddTest(new DateTime(2016, 09, 19), 257686, 257686);
            AddTest(new DateTime(2016, 09, 16), 259419, 259419);
            AddTest(new DateTime(2016, 09, 15), 650245, 650245);
            AddTest(new DateTime(2016, 09, 14), 596062, 596062);
            AddTest(new DateTime(2016, 09, 13), 224815, 224815);
            AddTest(new DateTime(2016, 09, 12), 228630, 228630);
            AddTest(new DateTime(2016, 09, 09), 205580, 205580);
            AddTest(new DateTime(2016, 09, 08), 559772, 559772);
            AddTest(new DateTime(2016, 09, 07), 584525, 584525);
            AddTest(new DateTime(2016, 09, 06), 246551, 246551);
            AddTest(new DateTime(2016, 09, 05), 229221, 229221);
            AddTest(new DateTime(2016, 09, 02), 239995, 239995);
            AddTest(new DateTime(2016, 09, 01), 463660, 463660);
            AddTest(new DateTime(2016, 08, 31), 476257, 476257);
            AddTest(new DateTime(2016, 08, 30), 228403, 228403);
            AddTest(new DateTime(2016, 08, 29), 202680, 202680);
            AddTest(new DateTime(2016, 08, 26), 220247, 220247);
            AddTest(new DateTime(2016, 08, 25), 502590, 502590);
            AddTest(new DateTime(2016, 08, 24), 501035, 501035);
            AddTest(new DateTime(2016, 08, 23), 223857, 223857);
            AddTest(new DateTime(2016, 08, 22), 212927, 212927);
            AddTest(new DateTime(2016, 08, 19), 179888, 179888);
            AddTest(new DateTime(2016, 08, 18), 368938, 368938);
            AddTest(new DateTime(2016, 08, 17), 374521, 374521);
            AddTest(new DateTime(2016, 08, 16), 186891, 186891);
            AddTest(new DateTime(2016, 08, 15), 179619, 179619);
            AddTest(new DateTime(2016, 08, 12), 162360, 162360);
            AddTest(new DateTime(2016, 08, 11), 393273, 393273);
            AddTest(new DateTime(2016, 08, 10), 376255, 376255);
            AddTest(new DateTime(2016, 08, 09), 151499, 151499);
            AddTest(new DateTime(2016, 08, 08), 138862, 138862);
            AddTest(new DateTime(2016, 08, 05), 125059, 125059);
            AddTest(new DateTime(2016, 08, 04), 338763, 338763);
            AddTest(new DateTime(2016, 08, 03), 388111, 388111);
            AddTest(new DateTime(2016, 08, 02), 203524, 203524);
            AddTest(new DateTime(2016, 08, 01), 296057, 296057);
            AddTest(new DateTime(2016, 07, 29), 281734, 281734);
            AddTest(new DateTime(2016, 07, 28), 496027, 496027);
            AddTest(new DateTime(2016, 07, 27), 446002, 446002);
            AddTest(new DateTime(2016, 07, 26), 187555, 187555);
            AddTest(new DateTime(2016, 07, 25), 175495, 175495);
            AddTest(new DateTime(2016, 07, 22), 165720, 165720);
            AddTest(new DateTime(2016, 07, 21), 419218, 419218);
            AddTest(new DateTime(2016, 07, 20), 413422, 413422);
            AddTest(new DateTime(2016, 07, 19), 178865, 178865);
            AddTest(new DateTime(2016, 07, 18), 222475, 222475);
            AddTest(new DateTime(2016, 07, 15), 183069, 183069);
            AddTest(new DateTime(2016, 07, 14), 495681, 495681);
            AddTest(new DateTime(2016, 07, 13), 507611, 507611);
            AddTest(new DateTime(2016, 07, 12), 129330, 129330);
            AddTest(new DateTime(2016, 07, 11), 104663, 104663);
            AddTest(new DateTime(2016, 07, 08), 293506, 293506);
            AddTest(new DateTime(2016, 07, 07), 314605, 314605);
            AddTest(new DateTime(2016, 07, 06), 137482, 137482);
            AddTest(new DateTime(2016, 07, 05), 145778, 145778);
            AddTest(new DateTime(2016, 07, 04), 324552, 324552);
            AddTest(new DateTime(2016, 07, 01), 313909, 313909);
            AddTest(new DateTime(2016, 06, 30), 136109, 136109);
            AddTest(new DateTime(2016, 06, 29), 88442, 88442);
            AddTest(new DateTime(2016, 06, 28), 99319, 99319);
            AddTest(new DateTime(2016, 06, 27), 466376, 466376);
            AddTest(new DateTime(2016, 06, 24), 434043, 434043);
            AddTest(new DateTime(2016, 06, 23), 134237, 134237);
            AddTest(new DateTime(2016, 06, 22), 111278, 111278);
            AddTest(new DateTime(2016, 06, 21), 96282, 96282);
            AddTest(new DateTime(2016, 06, 20), 229356, 229356);
            AddTest(new DateTime(2016, 06, 17), 196043, 196043);
            AddTest(new DateTime(2016, 06, 16), 58710, 58710);
            AddTest(new DateTime(2016, 06, 15), 73901, 73901);
            AddTest(new DateTime(2016, 06, 14), 64764, 64764);
            AddTest(new DateTime(2016, 06, 13), 319267, 319267);
            AddTest(new DateTime(2016, 06, 10), 332416, 332416);
            AddTest(new DateTime(2016, 06, 09), 122764, 122764);
            AddTest(new DateTime(2016, 06, 08), 109951, 109951);
            AddTest(new DateTime(2016, 06, 07), 106038, 106038);
            AddTest(new DateTime(2016, 06, 06), 327605, 327605);
            AddTest(new DateTime(2016, 06, 03), 301846, 301846);
            AddTest(new DateTime(2016, 06, 02), 43752, 43752);
            AddTest(new DateTime(2016, 06, 01), 24068, 24068);
            AddTest(new DateTime(2016, 05, 31), 28932, 28932);
            AddTest(new DateTime(2016, 05, 30), 142565, 142565);
            AddTest(new DateTime(2016, 05, 27), 138791, 138791);
            AddTest(new DateTime(2016, 05, 26), 5350, 5350);
            AddTest(new DateTime(2016, 05, 25), 5644, 5644);
            AddTest(new DateTime(2016, 05, 24), 6955, 6955);
            AddTest(new DateTime(2016, 05, 23), 16674, 16674);
            AddTest(new DateTime(2016, 05, 20), 1833, 1833);
            AddTest(new DateTime(2016, 05, 19), 5650, 5650);
            AddTest(new DateTime(2016, 05, 18), 54113, 54113);
            AddTest(new DateTime(2016, 05, 17), 66228, 66228);
            AddTest(new DateTime(2016, 05, 16), 239219, 239219);
            AddTest(new DateTime(2016, 05, 13), 267743, 267743);
            AddTest(new DateTime(2016, 05, 12), 121190, 121190);
            AddTest(new DateTime(2016, 05, 11), 109716, 109716);
            AddTest(new DateTime(2016, 05, 10), 95386, 95386);
            AddTest(new DateTime(2016, 05, 09), 195849, 195849);
            AddTest(new DateTime(2016, 05, 06), 158127, 158127);
            AddTest(new DateTime(2016, 05, 05), 39247, 39247);
            AddTest(new DateTime(2016, 05, 04), 71877, 71877);
            AddTest(new DateTime(2016, 05, 03), 87966, 87966);
            AddTest(new DateTime(2016, 05, 02), 304031, 304031);
            AddTest(new DateTime(2016, 04, 29), 315437, 315437);
            AddTest(new DateTime(2016, 04, 28), 120970, 120970);
            AddTest(new DateTime(2016, 04, 27), 121035, 121035);
            AddTest(new DateTime(2016, 04, 26), 111204, 111204);
            AddTest(new DateTime(2016, 04, 25), 253982, 253982);
            AddTest(new DateTime(2016, 04, 22), 229895, 229895);
            AddTest(new DateTime(2016, 04, 21), 84341, 84341);
            AddTest(new DateTime(2016, 04, 20), 90404, 90404);
            AddTest(new DateTime(2016, 04, 19), 87679, 87679);
            AddTest(new DateTime(2016, 04, 18), 155417, 155417);
            AddTest(new DateTime(2016, 04, 15), 182639, 182639);
            AddTest(new DateTime(2016, 04, 14), 108862, 108862);
            AddTest(new DateTime(2016, 04, 13), 112252, 112252);
            AddTest(new DateTime(2016, 04, 12), 120393, 120393);
            AddTest(new DateTime(2016, 04, 11), 312811, 312811);
            AddTest(new DateTime(2016, 04, 08), 315495, 315495);
            AddTest(new DateTime(2016, 04, 07), 152793, 152793);
            AddTest(new DateTime(2016, 04, 06), 146570, 146570);
            AddTest(new DateTime(2016, 04, 05), 113249, 113249);
            AddTest(new DateTime(2016, 04, 04), 270599, 270599);
            AddTest(new DateTime(2016, 04, 01), 234193, 234193);
            AddTest(new DateTime(2016, 03, 31), 61116, 61116);
            AddTest(new DateTime(2016, 03, 30), 87520, 87520);
            AddTest(new DateTime(2016, 03, 29), 112481, 112481);
            AddTest(new DateTime(2016, 03, 28), 180378, 180378);
            AddTest(new DateTime(2016, 03, 25), 134155, 134155);
            AddTest(new DateTime(2016, 03, 24), 62313, 62313);
            AddTest(new DateTime(2016, 03, 23), 77644, 77644);
            AddTest(new DateTime(2016, 03, 22), 96898, 96898);
            AddTest(new DateTime(2016, 03, 21), 136082, 136082);
            AddTest(new DateTime(2016, 03, 18), 136694, 136694);
            AddTest(new DateTime(2016, 03, 17), 97316, 97316);
            AddTest(new DateTime(2016, 03, 16), 84765, 84765);
            AddTest(new DateTime(2016, 03, 15), 5576, 5576);
            AddTest(new DateTime(2016, 03, 14), 14835, 14835);
            AddTest(new DateTime(2016, 03, 11), 4585, 4585);
            AddTest(new DateTime(2016, 03, 10), 40129, 40129);
            AddTest(new DateTime(2016, 03, 09), 33044, 33044);
            AddTest(new DateTime(2016, 03, 08), 40950, 40950);
            AddTest(new DateTime(2016, 03, 07), 148049, 148049);
            AddTest(new DateTime(2016, 03, 04), 135709, 135709);
            AddTest(new DateTime(2016, 03, 03), 33989, 33989);
            AddTest(new DateTime(2016, 03, 02), 1480, 1480);
            AddTest(new DateTime(2016, 03, 01), 3715, 3715);
            AddTest(new DateTime(2016, 02, 29), 40306, 40306);
            AddTest(new DateTime(2016, 02, 26), 28505, 28505);
            AddTest(new DateTime(2016, 02, 25), 4733, 4733);
            AddTest(new DateTime(2016, 02, 24), 1842, 1842);
            AddTest(new DateTime(2016, 02, 23), 2907, 2907);
            AddTest(new DateTime(2016, 02, 22), 5452, 5452);
            AddTest(new DateTime(2016, 02, 19), 13342, 13342);
            AddTest(new DateTime(2016, 02, 18), 6298, 6298);
            AddTest(new DateTime(2016, 02, 17), 9084, 9084);
            AddTest(new DateTime(2016, 02, 16), 6091, 6091);
            AddTest(new DateTime(2016, 02, 15), 53032, 53032);
            AddTest(new DateTime(2016, 02, 12), 83151, 83151);
            AddTest(new DateTime(2016, 02, 11), 73274, 73274);
            AddTest(new DateTime(2016, 02, 10), 61856, 61856);
            AddTest(new DateTime(2016, 02, 09), 25680, 25680);
            AddTest(new DateTime(2016, 02, 08), 180078, 180078);
            AddTest(new DateTime(2016, 02, 05), 223505, 223505);
            AddTest(new DateTime(2016, 02, 04), 52691, 52691);
            AddTest(new DateTime(2016, 02, 03), 39922, 39922);
            AddTest(new DateTime(2016, 02, 02), 29183, 29183);
            AddTest(new DateTime(2016, 02, 01), 5056, 5056);
            AddTest(new DateTime(2016, 01, 29), 5115, 5115);
            AddTest(new DateTime(2016, 01, 28), 32832, 32832);
            AddTest(new DateTime(2016, 01, 27), 18599, 18599);
            AddTest(new DateTime(2016, 01, 26), 43640, 43640);
            AddTest(new DateTime(2016, 01, 25), 127632, 127632);
            AddTest(new DateTime(2016, 01, 22), 125955, 125955);
            AddTest(new DateTime(2016, 01, 21), 49906, 49906);
            AddTest(new DateTime(2016, 01, 20), 8009, 8009);
            AddTest(new DateTime(2016, 01, 19), 6048, 6048);
            AddTest(new DateTime(2016, 01, 18), 116631, 116631);
            AddTest(new DateTime(2016, 01, 15), 78458, 78458);
            AddTest(new DateTime(2016, 01, 14), 214, 214);
            AddTest(new DateTime(2016, 01, 13), 17386, 17386);
            AddTest(new DateTime(2016, 01, 12), 17432, 17432);
            AddTest(new DateTime(2016, 01, 11), 19745, 19745);
            AddTest(new DateTime(2016, 01, 08), 32635, 32635);
            AddTest(new DateTime(2016, 01, 07), 47278, 47278);
            AddTest(new DateTime(2016, 01, 06), 48620, 48620);
            AddTest(new DateTime(2016, 01, 05), 47180, 47180);
            AddTest(new DateTime(2016, 01, 04), 76564, 76564);
            AddTest(new DateTime(2016, 01, 01), 56517, 56517);
            AddTest(new DateTime(2015, 12, 31), 5578, 5578);
            AddTest(new DateTime(2015, 12, 30), 1587, 1587);
            AddTest(new DateTime(2015, 12, 29), 16336, 16336);
            AddTest(new DateTime(2015, 12, 28), 164658, 164658);
            AddTest(new DateTime(2015, 12, 25), 182700, 182700);
            AddTest(new DateTime(2015, 12, 24), 65333, 65333);
            AddTest(new DateTime(2015, 12, 23), 17090, 17090);
            AddTest(new DateTime(2015, 12, 22), 912, 912);
            AddTest(new DateTime(2015, 12, 21), 27596, 27596);
            AddTest(new DateTime(2015, 12, 18), 40899, 40899);
            AddTest(new DateTime(2015, 12, 17), 15335, 15335);
            AddTest(new DateTime(2015, 12, 16), 26790, 26790);
            AddTest(new DateTime(2015, 12, 15), 34364, 34364);
            AddTest(new DateTime(2015, 12, 14), 11282, 11282);
            AddTest(new DateTime(2015, 12, 11), 17818, 17818);
            AddTest(new DateTime(2015, 12, 10), 8026, 8026);
            AddTest(new DateTime(2015, 12, 09), 1249, 1249);
            AddTest(new DateTime(2015, 12, 08), 3617, 3617);
            AddTest(new DateTime(2015, 12, 07), 7152, 7152);
            AddTest(new DateTime(2015, 12, 04), 20125, 20125);
            AddTest(new DateTime(2015, 12, 03), 60860, 60860);
            AddTest(new DateTime(2015, 12, 02), 13785, 13785);
            AddTest(new DateTime(2015, 12, 01), 4360, 4360);
            AddTest(new DateTime(2015, 11, 30), 10473, 10473);
            AddTest(new DateTime(2015, 11, 27), 710, 710);
            AddTest(new DateTime(2015, 11, 26), 32135, 32135);
            AddTest(new DateTime(2015, 11, 25), 774, 774);
            AddTest(new DateTime(2015, 11, 24), 669, 669);
            AddTest(new DateTime(2015, 11, 23), 68978, 68978);
            AddTest(new DateTime(2015, 11, 20), 89105, 89105);
            AddTest(new DateTime(2015, 11, 19), 12763, 12763);
            AddTest(new DateTime(2015, 11, 18), 2185, 2185);
            AddTest(new DateTime(2015, 11, 17), 15180, 15180);
            AddTest(new DateTime(2015, 11, 16), 105116, 105116);
            AddTest(new DateTime(2015, 11, 13), 63962, 63962);
            AddTest(new DateTime(2015, 11, 12), 42544, 42544);
            AddTest(new DateTime(2015, 11, 11), 74969, 74969);
            AddTest(new DateTime(2015, 11, 10), 15364, 15364);
            AddTest(new DateTime(2015, 11, 09), 4569, 4569);
            AddTest(new DateTime(2015, 11, 06), 30568, 30568);
            AddTest(new DateTime(2015, 11, 05), 18214, 18214);
            AddTest(new DateTime(2015, 11, 04), 6095, 6095);
            AddTest(new DateTime(2015, 11, 03), 91570, 91570);
            AddTest(new DateTime(2015, 11, 02), 90130, 90130);
            AddTest(new DateTime(2015, 10, 30), 32924, 32924);
            AddTest(new DateTime(2015, 10, 29), 24760, 24760);
            AddTest(new DateTime(2015, 10, 28), 3059, 3059);
            AddTest(new DateTime(2015, 10, 27), 22974, 22974);
            AddTest(new DateTime(2015, 10, 26), 14908, 14908);
            AddTest(new DateTime(2015, 10, 23), 142, 142);
            AddTest(new DateTime(2015, 10, 22), 25242, 25242);
            AddTest(new DateTime(2015, 10, 21), 147375, 147375);
            AddTest(new DateTime(2015, 10, 20), 184384, 184384);
            AddTest(new DateTime(2015, 10, 19), 50163, 50163);
            AddTest(new DateTime(2015, 10, 16), 4581, 4581);
            AddTest(new DateTime(2015, 10, 15), 7151, 7151);
            AddTest(new DateTime(2015, 10, 14), 11440, 11440);
            AddTest(new DateTime(2015, 10, 13), 16104, 16104);
            AddTest(new DateTime(2015, 10, 12), 8800, 8800);
            AddTest(new DateTime(2015, 10, 09), 21287, 21287);
            AddTest(new DateTime(2015, 10, 08), 34185, 34185);
            AddTest(new DateTime(2015, 10, 07), 47098, 47098);
            AddTest(new DateTime(2015, 10, 06), 33312, 33312);
            AddTest(new DateTime(2015, 10, 05), 35519, 35519);
            AddTest(new DateTime(2015, 10, 02), 26501, 26501);
            AddTest(new DateTime(2015, 10, 01), 166415, 166415);
            AddTest(new DateTime(2015, 09, 30), 157120, 157120);
            AddTest(new DateTime(2015, 09, 29), 19064, 19064);
            AddTest(new DateTime(2015, 09, 28), 37744, 37744);
            AddTest(new DateTime(2015, 09, 25), 22588, 22588);
            AddTest(new DateTime(2015, 09, 24), 20547, 20547);
            AddTest(new DateTime(2015, 09, 23), 10114, 10114);
            AddTest(new DateTime(2015, 09, 22), 15045, 15045);
            AddTest(new DateTime(2015, 09, 21), 11966, 11966);
            AddTest(new DateTime(2015, 09, 18), 27354, 27354);
            AddTest(new DateTime(2015, 09, 17), 30071, 30071);
            AddTest(new DateTime(2015, 09, 16), 15574, 15574);
            AddTest(new DateTime(2015, 09, 15), 1387, 1387);
            AddTest(new DateTime(2015, 09, 14), 2466, 2466);
            AddTest(new DateTime(2015, 09, 11), 11306, 11306);
            AddTest(new DateTime(2015, 09, 10), 46347, 46347);
            AddTest(new DateTime(2015, 09, 09), 66587, 66587);
            AddTest(new DateTime(2015, 09, 08), 21137, 21137);
            AddTest(new DateTime(2015, 09, 07), 4388, 4388);
            AddTest(new DateTime(2015, 09, 04), 533, 533);
            AddTest(new DateTime(2015, 09, 03), 71850, 71850);
            AddTest(new DateTime(2015, 09, 02), 94984, 94984);
            AddTest(new DateTime(2015, 09, 01), 39277, 39277);
            AddTest(new DateTime(2015, 08, 31), 28677, 28677);
            AddTest(new DateTime(2015, 08, 28), 17086, 17086);
            AddTest(new DateTime(2015, 08, 27), 20832, 20832);
            AddTest(new DateTime(2015, 08, 26), 14492, 14492);
            AddTest(new DateTime(2015, 08, 25), 23155, 23155);
            AddTest(new DateTime(2015, 08, 24), 23061, 23061);
            AddTest(new DateTime(2015, 08, 21), 25399, 25399);
            AddTest(new DateTime(2015, 08, 20), 4590, 4590);
            AddTest(new DateTime(2015, 08, 19), 17881, 17881);
            AddTest(new DateTime(2015, 08, 18), 30420, 30420);
            AddTest(new DateTime(2015, 08, 17), 38422, 38422);
            AddTest(new DateTime(2015, 08, 14), 35358, 35358);
            AddTest(new DateTime(2015, 08, 13), 39200, 39200);
            AddTest(new DateTime(2015, 08, 12), 74879, 74879);
            AddTest(new DateTime(2015, 08, 11), 23038, 23038);
            AddTest(new DateTime(2015, 08, 10), 5612, 5612);
            AddTest(new DateTime(2015, 08, 07), 13836, 13836);
            AddTest(new DateTime(2015, 08, 06), 20406, 20406);
            AddTest(new DateTime(2015, 08, 05), 25737, 25737);
            AddTest(new DateTime(2015, 08, 04), 23206, 23206);
            #endregion Adding Exposure
        }

        /// <summary>
        /// Ajoute ou met à jour les valeurs de test d'Exposure et de T0Exposure à une date
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pExposure"></param>
        /// <param name="pT0Exposure"></param>
        /// <returns>true si les valeurs a été ajoutée ou mise à jour</returns>
        public bool AddTest(DateTime pDate, decimal pExposure, decimal pT0Exposure)
        {
            if (pDate == m_IMSMPreviousDate)
            {
                m_T0Exposure = pT0Exposure;
            }
            return Add(m_Exposure, pDate, pExposure);
        }
        #endregion Test Methods
    }

    /// <summary>
    /// Class des paramètres de calcul de l'IMSM
    /// </summary>
    internal class IMSMParameter
    {
        #region Members
        // PM 20180316 [23840] Ajout version
        private decimal m_MethodVersion;
        private bool m_IsWithHolidayAdjustment;
        private int m_WindowSizeStatistic;
        private int m_WindowSizeMaximum;
        private decimal m_EWMAFactor;
        private decimal m_Alpha;
        private decimal m_Beta;
        private decimal m_MinIMSMInitial;
        private int m_MinIMSMInitialWindowSize;
        private decimal m_MinIMSM;
        private readonly IMSMSecurityAddon m_SecurityAddon = new IMSMSecurityAddon();
        private readonly IMSMHolidayAdjustment m_HolidayAdjustment = new IMSMHolidayAdjustment();
        /// <summary>
        /// Calcul uniquement du Current Exposure Spot Margin de l'ECC
        /// </summary>
        // PM 20200910 [25482] Ajout IsCalcCESMOnly
        private bool m_IsCalcCESMOnly;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Version de la méthode
        /// </summary>
        // PM 20180316 [23840] Ajout
        public decimal MethodVersion
        { get { return m_MethodVersion; } }

        /// <summary>
        /// Indicateur de prise en considération des jours fériés
        /// </summary>
        public bool IsWithHolidayAdjustment
        { get { return m_IsWithHolidayAdjustment; } }

        /// <summary>
        /// Taille de la fenêtre de prise en compte des données statistiques
        /// </summary>
        public int WindowSizeStatistic
        { get { return m_WindowSizeStatistic; } }

        /// <summary>
        /// Taille de la fenêtre de la partie maximum du calcul de l'IMSM
        /// </summary>
        public int WindowSizeMaximum
        { get { return m_WindowSizeMaximum; } }

        /// <summary>
        /// Paramètre EWMA (Exponentially Weighted Moving Average / Moyennes Mobiles Pondérées Expontiellement) pour calculer la volatilité dans le calcul de l'écart-type
        /// </summary>
        public decimal EWMAFactor
        { get { return m_EWMAFactor; } }

        /// <summary>
        /// Multiplicateur pour l'écart-type utilisé dans la partie statistique du calcul de l'IMSM
        /// </summary>
        public decimal Alpha
        { get { return m_Alpha; } }

        /// <summary>
        /// Multiplicateur pour la partie déterministe du calcul de l'IMSM
        /// </summary>
        public decimal Beta
        { get { return m_Beta; } }

        /// <summary>
        /// Minimum absolu initial de l'IMSM pour les "MinIMSMInitialWindowSize"(30) premiers jours après l'admission
        /// </summary>
        public decimal MinIMSMInitial
        { get { return m_MinIMSMInitial; } }

        /// <summary>
        /// Nombre de jours pendant lesquels appliquer le montant minimum absolu initial de l'IMSM
        /// </summary>
        public int MinIMSMInitialWindowSize
        { get { return m_MinIMSMInitialWindowSize; } }

        /// <summary>
        /// Minimum absolu de l'IMSM
        /// </summary>
        public decimal MinIMSM
        { get { return m_MinIMSM; } }

        /// <summary>
        /// Facteurs de sécurité pour l'écart-type
        /// </summary>
        public IMSMSecurityAddon SecurityAddon
        { get { return m_SecurityAddon; } }

        /// <summary>
        /// Paramètres d'ajustement des jours fériés
        /// </summary>
        public IMSMHolidayAdjustment HolidayAdjustment
        { get { return m_HolidayAdjustment; } }

        /// <summary>
        /// Calcul uniquement du Current Exposure Spot Margin de l'ECC
        /// </summary>
        // PM 20200910 [25482] Ajout IsCalcCESMOnly
        public bool IsCalcCESMOnly
        { get { return m_IsCalcCESMOnly; } }
        #endregion Accessors

        #region Methods
        /// <summary>
        /// Initialiser les paramètres de calcul
        /// </summary>
        /// <param name="pMethodParameter"></param>
        /// <param name="pHolidayAdjParameter"></param>
        /// <param name="pAddonParameter"></param>
        /// <returns></returns>
        /// PM 20170808 [23371] Ajout paramètre pCESMMarginParameter
        public IMSMGlobalParameterCom SetParameters(ImMethodParameter pMethodParameter,
                    IEnumerable<IMSMHolidayAdjustmentParameter> pHolidayAdjParameter,
                    IEnumerable<IMSMSecurityAddonParameter> pAddonParameter)
        {
            IMSMGlobalParameterCom com = new IMSMGlobalParameterCom();

            if (pMethodParameter != default(ImMethodParameter))
            {
                // PM 20180316 [23840] Ajout version
                m_MethodVersion = pMethodParameter.MethodVersion;
                //
                m_IsWithHolidayAdjustment = pMethodParameter.IsWithHolidayAdjustment;
                m_WindowSizeStatistic = pMethodParameter.WindowSizeStatistic;
                m_WindowSizeMaximum = pMethodParameter.WindowSizeForMax;
                m_EWMAFactor = pMethodParameter.EwmaFactor;
                m_Alpha = pMethodParameter.AlphaFactor;
                m_Beta = pMethodParameter.BetaFactor;
                m_MinIMSMInitial = pMethodParameter.MinimumAmountFirst;
                m_MinIMSMInitialWindowSize = pMethodParameter.MinimumAmountFirstTerm;
                m_MinIMSM = pMethodParameter.MinimumAmount;
                // PM 20200910 [25482] Ajout m_IsCalcCESMOnly
                m_IsCalcCESMOnly = pMethodParameter.IsCalcCESMOnly;

                // Com
                com.IsWithHolidayAdjustment = m_IsWithHolidayAdjustment;
                com.WindowSizeStatistic = m_WindowSizeStatistic;
                com.WindowSizeMaximum = m_WindowSizeMaximum;
                com.EWMAFactor = EWMAFactor;
                com.Alpha = m_Alpha;
                com.Beta = m_Beta;
                com.MinIMSMInitial = m_MinIMSMInitial;
                com.MinIMSMInitialWindowSize = MinIMSMInitialWindowSize;
                com.MinIMSM = MinIMSM;
                // PM 20200910 [25482] Ajout IsCalcCESMOnly
                com.IsCalcCESMOnly = IsCalcCESMOnly;
            }
            if (pHolidayAdjParameter != default)
            {
                m_HolidayAdjustment.SetAdjustment(pHolidayAdjParameter);
            }
            if (pAddonParameter != default)
            {
                m_SecurityAddon.SetAddon(pAddonParameter);
            }
            return com;
        }

        /// <summary>
        /// Supprime toutes les valeurs des paramètres de calcul
        /// </summary>
        public void Clear()
        {
            m_IsWithHolidayAdjustment = false;
            m_WindowSizeStatistic = 0;
            m_WindowSizeMaximum = 0;
            m_EWMAFactor = 0;
            m_Alpha = 0;
            m_Beta = 0;
            m_MinIMSMInitial = 0;
            m_MinIMSMInitialWindowSize = 0;
            m_MinIMSM = 0;
            m_SecurityAddon.Clear();
            m_HolidayAdjustment.Clear();
        }
        #endregion Methods

        #region Test Methods
        /// <summary>
        /// Initialise les paramètres de calcul avec les valeurs de test
        /// </summary>
        public void SetTestParameter()
        {
            m_IsWithHolidayAdjustment = true;
            m_WindowSizeStatistic = 250;
            m_WindowSizeMaximum = 20;
            m_EWMAFactor = 0.99m;
            m_Alpha = 3.1m;
            m_Beta = 1.4m;
            m_MinIMSMInitial = 30000;
            m_MinIMSMInitialWindowSize = 30;
            m_MinIMSM = 10000;
            m_SecurityAddon.SetTestAddon();
            m_HolidayAdjustment.SetTestAdjustment();
        }
        #endregion Test Methods
    }

    /// <summary>
    /// Class des facteurs de sécurité pour l'écart-type
    /// </summary>
    internal class IMSMSecurityAddon
    {
        #region Members
        private Dictionary<int, decimal> m_SecurityAddon = new Dictionary<int, decimal>();
        #endregion Members

        #region Accessors
        /// <summary>
        /// Ensemble des facteurs de sécurité pour l'écart-type
        /// </summary>
        public Dictionary<int, decimal> SecurityAddon
        { get { return m_SecurityAddon; } }
        #endregion Accessors

        #region Methods
        /// <summary>
        /// Ajout ou mise à jour d'un facteur de sécurité pour un point donné
        /// </summary>
        /// <param name="pDatapoint"></param>
        /// <param name="pAddon"></param>
        /// <returns></returns>
        public bool SetAddon(int pDatapoint, decimal pAddon)
        {
            bool alreadyExists = m_SecurityAddon.ContainsKey(pDatapoint);
            if (alreadyExists)
            {
                m_SecurityAddon[pDatapoint] = pAddon;
            }
            else
            {
                m_SecurityAddon.Add(pDatapoint, pAddon);
            }
            return alreadyExists;
        }

        /// <summary>
        /// Initialise les facteurs de sécurité
        /// </summary>
        /// <param name="pAddonParameter"></param>
        public void SetAddon(IEnumerable<IMSMSecurityAddonParameter> pAddonParameter)
        {
            if (pAddonParameter != default)
            {
                m_SecurityAddon.Clear();
                m_SecurityAddon = pAddonParameter.ToDictionary(k => k.DataPoint, e => e.SecurityAddon);
            }
        }

        /// <summary>
        /// Lecture d'un facteur de sécurité pour un point donné
        /// </summary>
        /// <param name="pDatapoint"></param>
        /// <returns></returns>
        public decimal GetAddon(int pDatapoint)
        {
            m_SecurityAddon.TryGetValue(pDatapoint, out decimal addon);
            return addon;
        }

        /// <summary>
        /// Supprime toutes les valeurs de SecurityAddon
        /// </summary>
        public void Clear()
        {
            m_SecurityAddon.Clear();
        }
        #endregion Methods

        #region Test Methods
        /// <summary>
        /// Initialisation des facteurs de sécurité avec les valeurs de test
        /// </summary>
        public void SetTestAddon()
        {
            Clear();
            SetAddon(0, 0m);
            SetAddon(1, 0m);
            SetAddon(2, 1.646453192m);
            SetAddon(3, 1.515964294m);
            SetAddon(4, 1.443167374m);
            SetAddon(5, 1.394246774m);
            SetAddon(6, 1.359192892m);
            SetAddon(7, 1.332964932m);
            SetAddon(8, 1.310597544m);
            SetAddon(9, 1.292426084m);
            SetAddon(10, 1.27705215m);
            SetAddon(11, 1.264419457m);
            SetAddon(12, 1.253459254m);
            SetAddon(13, 1.243358109m);
            SetAddon(14, 1.234947863m);
            SetAddon(15, 1.226157543m);
            SetAddon(16, 1.219659221m);
            SetAddon(17, 1.213080457m);
            SetAddon(18, 1.20749661m);
            SetAddon(19, 1.202012485m);
            SetAddon(20, 1.197029804m);
            SetAddon(21, 1.192173406m);
            SetAddon(22, 1.188140008m);
            SetAddon(23, 1.18407171m);
            SetAddon(24, 1.1800214m);
            SetAddon(25, 1.17652192m);
            SetAddon(26, 1.173119982m);
            SetAddon(27, 1.169535978m);
            SetAddon(28, 1.16730325m);
            SetAddon(29, 1.16416477m);
            SetAddon(30, 1.161422076m);
            SetAddon(31, 1.159151541m);
            SetAddon(32, 1.156164552m);
            SetAddon(33, 1.153909269m);
            SetAddon(34, 1.152081506m);
            SetAddon(35, 1.149936078m);
            SetAddon(36, 1.148178473m);
            SetAddon(37, 1.145984481m);
            SetAddon(38, 1.144232634m);
            SetAddon(39, 1.142550115m);
            SetAddon(40, 1.140381755m);
            SetAddon(41, 1.138932948m);
            SetAddon(42, 1.137323189m);
            SetAddon(43, 1.136134974m);
            SetAddon(44, 1.134463909m);
            SetAddon(45, 1.133215074m);
            SetAddon(46, 1.131756861m);
            SetAddon(47, 1.130307484m);
            SetAddon(48, 1.128906158m);
            SetAddon(49, 1.127649298m);
            SetAddon(50, 1.12640884m);
            SetAddon(51, 1.125386398m);
            SetAddon(52, 1.124343032m);
            SetAddon(53, 1.123094251m);
            SetAddon(54, 1.121933411m);
            SetAddon(55, 1.120840122m);
            SetAddon(56, 1.120174045m);
            SetAddon(57, 1.11905813m);
            SetAddon(58, 1.117897335m);
            SetAddon(59, 1.117025692m);
            SetAddon(60, 1.116399872m);
            SetAddon(61, 1.115277009m);
            SetAddon(62, 1.114278224m);
            SetAddon(63, 1.113367078m);
            SetAddon(64, 1.112461063m);
            SetAddon(65, 1.111701726m);
            SetAddon(66, 1.111298376m);
            SetAddon(67, 1.110261213m);
            SetAddon(68, 1.109716978m);
            SetAddon(69, 1.108906923m);
            SetAddon(70, 1.107989669m);
            SetAddon(71, 1.107557447m);
            SetAddon(72, 1.106707429m);
            SetAddon(73, 1.106180878m);
            SetAddon(74, 1.105297908m);
            SetAddon(75, 1.104797033m);
            SetAddon(76, 1.104398777m);
            SetAddon(77, 1.103875682m);
            SetAddon(78, 1.103152124m);
            SetAddon(79, 1.102164867m);
            SetAddon(80, 1.101915517m);
            SetAddon(81, 1.101442752m);
            SetAddon(82, 1.100746263m);
            SetAddon(83, 1.100381599m);
            SetAddon(84, 1.099932617m);
            SetAddon(85, 1.099431192m);
            SetAddon(86, 1.098990387m);
            SetAddon(87, 1.098312874m);
            SetAddon(88, 1.097774925m);
            SetAddon(89, 1.097347237m);
            SetAddon(90, 1.096786018m);
            SetAddon(91, 1.096363538m);
            SetAddon(92, 1.095816387m);
            SetAddon(93, 1.095574372m);
            SetAddon(94, 1.094910627m);
            SetAddon(95, 1.094169373m);
            SetAddon(96, 1.094010116m);
            SetAddon(97, 1.093656565m);
            SetAddon(98, 1.093218449m);
            SetAddon(99, 1.092988967m);
            SetAddon(100, 1.092344007m);
            SetAddon(101, 1.092125249m);
            SetAddon(102, 1.091962403m);
            SetAddon(103, 1.091401331m);
            SetAddon(104, 1.09106148m);
            SetAddon(105, 1.090662343m);
            SetAddon(106, 1.090517309m);
            SetAddon(107, 1.089966874m);
            SetAddon(108, 1.089572751m);
            SetAddon(109, 1.089322678m);
            SetAddon(110, 1.08891309m);
            SetAddon(111, 1.088820834m);
            SetAddon(112, 1.08837003m);
            SetAddon(113, 1.08792234m);
            SetAddon(114, 1.087736885m);
            SetAddon(115, 1.087490052m);
            SetAddon(116, 1.087257512m);
            SetAddon(117, 1.086835198m);
            SetAddon(118, 1.08640167m);
            SetAddon(119, 1.086475765m);
            SetAddon(120, 1.086129878m);
            SetAddon(121, 1.085839042m);
            SetAddon(122, 1.085376289m);
            SetAddon(123, 1.085076985m);
            SetAddon(124, 1.084844767m);
            SetAddon(125, 1.084798844m);
            SetAddon(126, 1.084302369m);
            SetAddon(127, 1.084055707m);
            SetAddon(128, 1.083857398m);
            SetAddon(129, 1.083424791m);
            SetAddon(130, 1.083437832m);
            SetAddon(131, 1.082939711m);
            SetAddon(132, 1.082885456m);
            SetAddon(133, 1.082675067m);
            SetAddon(134, 1.082346582m);
            SetAddon(135, 1.082172302m);
            SetAddon(136, 1.082031482m);
            SetAddon(137, 1.081808674m);
            SetAddon(138, 1.081677053m);
            SetAddon(139, 1.081292807m);
            SetAddon(140, 1.081055471m);
            SetAddon(141, 1.080891304m);
            SetAddon(142, 1.080688234m);
            SetAddon(143, 1.08053619m);
            SetAddon(144, 1.080261387m);
            SetAddon(145, 1.080137952m);
            SetAddon(146, 1.079993529m);
            SetAddon(147, 1.079667078m);
            SetAddon(148, 1.07944325m);
            SetAddon(149, 1.079225752m);
            SetAddon(150, 1.079087189m);
            SetAddon(151, 1.079024375m);
            SetAddon(152, 1.078881046m);
            SetAddon(153, 1.078665125m);
            SetAddon(154, 1.078420245m);
            SetAddon(155, 1.07810761m);
            SetAddon(156, 1.078298085m);
            SetAddon(157, 1.078027971m);
            SetAddon(158, 1.077604829m);
            SetAddon(159, 1.077607536m);
            SetAddon(160, 1.077456396m);
            SetAddon(161, 1.077404123m);
            SetAddon(162, 1.077011054m);
            SetAddon(163, 1.076995277m);
            SetAddon(164, 1.076901285m);
            SetAddon(165, 1.07677353m);
            SetAddon(166, 1.076501754m);
            SetAddon(167, 1.07620542m);
            SetAddon(168, 1.076281379m);
            SetAddon(169, 1.076099727m);
            SetAddon(170, 1.075890158m);
            SetAddon(171, 1.075756636m);
            SetAddon(172, 1.075786598m);
            SetAddon(173, 1.075516533m);
            SetAddon(174, 1.075558822m);
            SetAddon(175, 1.075476649m);
            SetAddon(176, 1.075136045m);
            SetAddon(177, 1.075058171m);
            SetAddon(178, 1.07507848m);
            SetAddon(179, 1.074618834m);
            SetAddon(180, 1.074441295m);
            SetAddon(181, 1.074639307m);
            SetAddon(182, 1.074270758m);
            SetAddon(183, 1.074295996m);
            SetAddon(184, 1.074341677m);
            SetAddon(185, 1.074040246m);
            SetAddon(186, 1.073899308m);
            SetAddon(187, 1.07361353m);
            SetAddon(188, 1.073729818m);
            SetAddon(189, 1.073667928m);
            SetAddon(190, 1.073389075m);
            SetAddon(191, 1.073414448m);
            SetAddon(192, 1.073224524m);
            SetAddon(193, 1.073224442m);
            SetAddon(194, 1.07296619m);
            SetAddon(195, 1.073031629m);
            SetAddon(196, 1.072936876m);
            SetAddon(197, 1.072723386m);
            SetAddon(198, 1.072544227m);
            SetAddon(199, 1.072609657m);
            SetAddon(200, 1.072520985m);
            SetAddon(201, 1.072276514m);
            SetAddon(202, 1.072316099m);
            SetAddon(203, 1.072036532m);
            SetAddon(204, 1.072106219m);
            SetAddon(205, 1.072071179m);
            SetAddon(206, 1.071963021m);
            SetAddon(207, 1.071785033m);
            SetAddon(208, 1.07187699m);
            SetAddon(209, 1.071624531m);
            SetAddon(210, 1.071489219m);
            SetAddon(211, 1.071461336m);
            SetAddon(212, 1.071471792m);
            SetAddon(213, 1.071415286m);
            SetAddon(214, 1.071269491m);
            SetAddon(215, 1.071010255m);
            SetAddon(216, 1.070969399m);
            SetAddon(217, 1.070949334m);
            SetAddon(218, 1.070600609m);
            SetAddon(219, 1.070668685m);
            SetAddon(220, 1.070489592m);
            SetAddon(221, 1.070593239m);
            SetAddon(222, 1.070656207m);
            SetAddon(223, 1.07067338m);
            SetAddon(224, 1.070361352m);
            SetAddon(225, 1.070302538m);
            SetAddon(226, 1.070317924m);
            SetAddon(227, 1.070246808m);
            SetAddon(228, 1.070072868m);
            SetAddon(229, 1.069950933m);
            SetAddon(230, 1.070003637m);
            SetAddon(231, 1.069980909m);
            SetAddon(232, 1.06982159m);
            SetAddon(233, 1.069781138m);
            SetAddon(234, 1.069827565m);
            SetAddon(235, 1.069737994m);
            SetAddon(236, 1.069690526m);
            SetAddon(237, 1.069655258m);
            SetAddon(238, 1.069480788m);
            SetAddon(239, 1.069224364m);
            SetAddon(240, 1.069252052m);
            SetAddon(241, 1.06922296m);
            SetAddon(242, 1.069136118m);
            SetAddon(243, 1.069105676m);
            SetAddon(244, 1.069060108m);
            SetAddon(245, 1.068997531m);
            SetAddon(246, 1.068756097m);
            SetAddon(247, 1.069078733m);
            SetAddon(248, 1.068958452m);
            SetAddon(249, 1.068819558m);
            SetAddon(250, 1.068589932m);
        }
        #endregion Test Methods
    }

    /// <summary>
    /// Class pour un paramètre d'ajustement de jour férié
    /// </summary>
    internal class IMSMHolidayAdjustmentValue
    {
        #region Members
        // PM 20231027 [XXXXX] Ajout m_IMSMCalculationDate
        private readonly DateTime m_IMSMCalculationDate;
        private readonly DateTime m_EffectiveDate;
        // PM 20231027 [XXXXX] Le Factor Lambda Holiday Adjustment passe de int à decimal
        private readonly decimal m_FactorLambda;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Date de calcul IMSM à laquelle se rapporte l'ajustement
        /// </summary>
        // PM 20231027 [XXXXX] Ajout IMSMCalculationDate
        public DateTime IMSMCalculationDate
        {
            get { return m_IMSMCalculationDate; }
        }
        /// <summary>
        /// Date de prise en compte des jours fériés qui ont précédés
        /// </summary>
        public DateTime EffectiveDate
        {
            get { return m_EffectiveDate; }
        }
        /// <summary>
        /// Nombre de jours fériés
        /// </summary>
        // PM 20231027 [XXXXX] Le Factor Lambda Holiday Adjustment passe de int à decimal
        public decimal FactorLambda
        {
            get { return m_FactorLambda; }
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pIMSMCalculationDate">Date de calcul IMSM à laquelle se rapporte l'ajustement</param>
        /// <param name="pEffectiveDate">Date effective à laquelle se rapporte l'ajustement</param>
        /// <param name="pFactorLambda"></param>
        // PM 20231027 [XXXXX] Le Factor Lambda Holiday Adjustment passe de int à decimal
        public IMSMHolidayAdjustmentValue(DateTime pIMSMCalculationDate, DateTime pEffectiveDate, decimal pFactorLambda)
        {
            m_IMSMCalculationDate = pIMSMCalculationDate;
            m_EffectiveDate = pEffectiveDate;
            m_FactorLambda = pFactorLambda;
        }
        #endregion Constructors
    }

    /// <summary>
    ///  Class des paramètres d'ajustement des jours fériés
    /// </summary>
    internal class IMSMHolidayAdjustment
    {
        #region Members
        private Dictionary<DateTime, IMSMHolidayAdjustmentValue> m_HolidayAdjustment = new Dictionary<DateTime, IMSMHolidayAdjustmentValue>();
        #endregion Members

        #region Accessors
        /// <summary>
        /// Ensemble des paramètres d'ajustement des jours fériés
        /// </summary>
        public Dictionary<DateTime, IMSMHolidayAdjustmentValue> HolidayAdjustment
        {
            get { return m_HolidayAdjustment; }
        }
        #endregion Accessors

        #region Methods
        /// <summary>
        /// Défini un paramètre d'ajustement de jours fériés
        /// </summary>
        /// <param name="pIMSMDate">Date de calcul IMSM à laquelle se rapporte l'ajustement</param>
        /// <param name="pEffectiveDate">Date effective à laquelle se rapporte l'ajustement</param>
        /// <param name="pAdjustment">Nombre de jour de l'ajustement</param>
        /// <returns></returns>
        public bool SetAdjustment(DateTime pIMSMDate, DateTime pEffectiveDate, int pAdjustment)
        {
            bool alreadyExists = m_HolidayAdjustment.ContainsKey(pIMSMDate);
            // PM 20231027 [XXXXX] Ajout pIMSMDate
            IMSMHolidayAdjustmentValue adjValue = new IMSMHolidayAdjustmentValue(pIMSMDate, pEffectiveDate, pAdjustment);
            if (alreadyExists)
            {
                m_HolidayAdjustment[pIMSMDate] = adjValue;
            }
            else
            {
                m_HolidayAdjustment.Add(pIMSMDate, adjValue);
            }
            return alreadyExists;
        }

        /// <summary>
        /// Initialise les ajustements des jours fériés
        /// </summary>
        /// <param name="pHolidayAdjParameter"></param>
        public void SetAdjustment(IEnumerable<IMSMHolidayAdjustmentParameter> pHolidayAdjParameter)
        {
            if (pHolidayAdjParameter != default)
            {
                m_HolidayAdjustment.Clear();
                // PM 20231027 [XXXXX] Ajout e.CalculationDate
                m_HolidayAdjustment = pHolidayAdjParameter.ToDictionary(k => k.CalculationDate, e => new IMSMHolidayAdjustmentValue(e.CalculationDate, e.EffectiveDate, e.LambdaFactor));
            }
        }

        /// <summary>
        /// Obtient la valeur d'ajustement pour une date de calcul IMSM
        /// </summary>
        /// <param name="pIMSMDate"></param>
        /// <returns></returns>
        // PM 20231027 [XXXXX] Le Factor Lambda Holiday Adjustment passe de int à decimal
        public decimal GetAdjustmentFromIMSMDate(DateTime pIMSMDate)
        {
            decimal adjustment = 0;
            if (m_HolidayAdjustment.TryGetValue(pIMSMDate, out IMSMHolidayAdjustmentValue adjValue))
            {
                adjustment = adjValue.FactorLambda;
            }
            return adjustment;
        }

        /// <summary>
        /// Obtient la valeur d'ajustement pour une date effective
        /// </summary>
        /// <param name="pDtEffective"></param>
        /// <returns></returns>
        // PM 20231027 [XXXXX] Le Factor Lambda Holiday Adjustment passe de int à decimal
        public decimal GetAdjustmentFromEffectiveDate(DateTime pDtEffective)
        {
            decimal adjustment = 0;
            IMSMHolidayAdjustmentValue adjValue = m_HolidayAdjustment.Values.FirstOrDefault(v => v.EffectiveDate == pDtEffective);
            if (adjValue != default(IMSMHolidayAdjustmentValue))
            {
                adjustment = adjValue.FactorLambda;
            }
            return adjustment;
        }

        /// <summary>
        /// Obtient la IMSM Calculation Date pour une date effective
        /// </summary>
        /// <param name="pDtEffective"></param>
        /// <returns></returns>
        // PM 20231027 [XXXXX] Ajout
        public DateTime GetIMSMDateFromEffectiveDate(DateTime pDtEffective)
        {
            DateTime IMSMDate = DateTime.MinValue;
            IMSMHolidayAdjustmentValue adjValue = m_HolidayAdjustment.Values.FirstOrDefault(v => v.EffectiveDate == pDtEffective);
            if (adjValue != default(IMSMHolidayAdjustmentValue))
            {
                IMSMDate = adjValue.IMSMCalculationDate;
            }
            return IMSMDate;
        }

        /// <summary>
        /// Supprime toutes les valeurs des ajustements de jours fériés
        /// </summary>
        public void Clear()
        {
            m_HolidayAdjustment.Clear();
        }
        #endregion Methods

        #region Test Methods
        /// <summary>
        /// Initialise les ajustements des jours fériés avec les valeurs de test
        /// </summary>
        public void SetTestAdjustment()
        {
            Clear();
            SetAdjustment(new DateTime(2015, 04, 01), new DateTime(2015, 04, 02), 2);
            SetAdjustment(new DateTime(2015, 04, 02), new DateTime(2015, 04, 07), 2);
            SetAdjustment(new DateTime(2015, 04, 29), new DateTime(2015, 04, 30), 1);
            SetAdjustment(new DateTime(2015, 04, 30), new DateTime(2015, 05, 04), 1);
            SetAdjustment(new DateTime(2015, 12, 23), new DateTime(2015, 12, 24), 1);
            SetAdjustment(new DateTime(2015, 12, 24), new DateTime(2015, 12, 28), 1);
            SetAdjustment(new DateTime(2015, 12, 30), new DateTime(2015, 12, 31), 1);
            SetAdjustment(new DateTime(2015, 12, 31), new DateTime(2016, 01, 04), 1);
            SetAdjustment(new DateTime(2016, 03, 23), new DateTime(2016, 03, 24), 2);
            SetAdjustment(new DateTime(2016, 03, 24), new DateTime(2016, 03, 29), 2);
            SetAdjustment(new DateTime(2016, 12, 22), new DateTime(2016, 12, 23), 1);
            SetAdjustment(new DateTime(2016, 12, 23), new DateTime(2016, 12, 27), 1);
            SetAdjustment(new DateTime(2017, 04, 12), new DateTime(2017, 04, 13), 2);
            SetAdjustment(new DateTime(2017, 04, 13), new DateTime(2017, 04, 18), 2);
            SetAdjustment(new DateTime(2017, 04, 27), new DateTime(2017, 04, 28), 1);
            SetAdjustment(new DateTime(2017, 04, 28), new DateTime(2017, 05, 02), 1);
            SetAdjustment(new DateTime(2017, 12, 21), new DateTime(2017, 12, 21), 2);
            SetAdjustment(new DateTime(2017, 12, 22), new DateTime(2017, 12, 27), 2);
            SetAdjustment(new DateTime(2017, 12, 28), new DateTime(2017, 12, 29), 1);
            SetAdjustment(new DateTime(2017, 12, 29), new DateTime(2018, 01, 02), 1);
            SetAdjustment(new DateTime(2018, 03, 28), new DateTime(2018, 03, 29), 2);
            SetAdjustment(new DateTime(2018, 03, 29), new DateTime(2018, 04, 03), 2);
            SetAdjustment(new DateTime(2019, 04, 17), new DateTime(2019, 04, 18), 2);
            SetAdjustment(new DateTime(2019, 04, 18), new DateTime(2019, 04, 23), 2);
            SetAdjustment(new DateTime(2020, 04, 08), new DateTime(2020, 04, 09), 2);
            SetAdjustment(new DateTime(2020, 04, 09), new DateTime(2020, 04, 14), 2);
            SetAdjustment(new DateTime(2020, 04, 29), new DateTime(2020, 04, 30), 1);
            SetAdjustment(new DateTime(2020, 04, 30), new DateTime(2020, 05, 04), 1);
            SetAdjustment(new DateTime(2020, 12, 23), new DateTime(2020, 12, 24), 1);
            SetAdjustment(new DateTime(2020, 12, 24), new DateTime(2020, 12, 28), 1);
            SetAdjustment(new DateTime(2020, 12, 30), new DateTime(2020, 12, 31), 1);
            SetAdjustment(new DateTime(2020, 12, 31), new DateTime(2021, 01, 04), 1);
        }
        #endregion Test Methods
    }

    /// <summary>
    /// Class de calcul de l'IMSM
    /// </summary>
    internal class IMSMCalculation
    {
        #region Members
        // Données d'entrées
        private IMSMExposure m_Exposure;
        private readonly IMSMParameter m_Parameter;
        // PM 20170602 [23212] Renommage de m_FirstActivityDate en m_AgreementDate
        private DateTime m_AgreementDate;
        //
        // Données de calculs
        private DateTime m_DateOfImsm;  // Date de calcul de l'IMSM
        private DateTime m_PreviousImsmDate;  // Date précédante de calcul de l'IMSM
        private Dictionary<DateTime, decimal> m_ExposureMoreThanMin;
        private Dictionary<DateTime, decimal> m_OneDayExposureMoreThanMin;
        private decimal m_HolidayAdjustment = 0;
        private decimal m_T0ExposurePreviousDay = 0;
        private decimal m_Mean = 0;
        private decimal m_StandardDeviation = 0;          // Standard Deviation
        private decimal m_SDS = 0;                        // Standard Deviation with Securityfactor
        private decimal m_MeanAlphaSDS = 0;
        private decimal m_BetaMax = 0;
        //
        // Journal du calcul
        private IMSMCalculationCom m_CalcLogCom;
        // Dictionnaire des taux de change réellement utile: Key = devise à convertir
        // PM 20230105 [26219] Déplacé à partir de la classe IMSMMethod
        private readonly Dictionary<string, IMExchangeRateParameterCom> m_UsedExchangeRateLog = default;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Données statistiques de l'exposition
        /// </summary>
        public IMSMExposure Exposure
        {
            get { return m_Exposure; }
        }

        /// <summary>
        /// Paramètres de calcul
        /// </summary>
        public IMSMParameter Parameter
        {
            get { return m_Parameter; }
        }

        /// <summary>
        /// Données du journal du calcul
        /// </summary>
        public IMSMCalculationCom LastCalculationCom
        {
            get { return m_CalcLogCom; }
        }

        /// <summary>
        /// Dictionnaire des taux de change réellement utile: Key = devise à convertir
        /// </summary>
        // PM 20230105 [26219] Ajout
        public Dictionary<string, IMExchangeRateParameterCom> UsedExchangeRateLog
        {
            get { return m_UsedExchangeRateLog; }
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public IMSMCalculation()
        {
            m_Parameter = new IMSMParameter();
            m_AgreementDate = DateTime.MinValue;

            // PM 20230105 [26219] Ajout
            m_UsedExchangeRateLog = new Dictionary<string, IMExchangeRateParameterCom>();
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Calcul de l'IMSM
        /// </summary>
        /// <param name="pDtImsm">Date valeur du montant de déposit (Date business + 1)</param>
        /// <param name="pAgreementDate">Date à laquelle l'agrément pour la chambre à eu lieu</param>
        /// <returns>Montant de déposit calculé</returns>
        public decimal Calc(DateTime pDtImsm, DateTime pAgreementDate)
        {
            // Nouvel objet pour le journal
            m_CalcLogCom = new IMSMCalculationCom();
            //
            // Initialisation du montant minimum de IMSM requis
            decimal imsmAmount = m_Parameter.MinIMSM;
            //
            // Initialisation de la date par defaut sur laquelle doit être constitué le montant de IMSM requis
            m_DateOfImsm = pDtImsm;
            // Initialisation de la date de début de l'activité
            m_AgreementDate = pAgreementDate;
            m_CalcLogCom.AgreementDate = m_AgreementDate;
            //
            if (m_Exposure != default(IMSMExposure))
            {
                // Test s'il y a plus de MinIMSMInitialWindowSize jours depuis la plus petite date d'Exposure
                if (m_Exposure.WindowDays.Where(d => (d >= m_AgreementDate)).Count() <= m_Parameter.MinIMSMInitialWindowSize)
                {
                    // Initialisation du montant minimum de IMSM requis pour début d'activité
                    imsmAmount = m_Parameter.MinIMSMInitial;
                }
                //
                // Recherche de la date réelle sur laquelle doit être constitué le montant de IMSM requis
                if (m_Parameter.IsWithHolidayAdjustment)
                {
                    // PM 20231027 [XXXXX] Le Factor Lambda Holiday Adjustment passe de int à decimal
                    decimal holydayAdj = m_Parameter.HolidayAdjustment.GetAdjustmentFromEffectiveDate(pDtImsm);
                    //if (holydayAdj > 0)
                    //{
                    //  m_DateOfImsm = IMSMTools.GetLessNDate(m_Exposure.WindowDays, pDtImsm, holydayAdj);
                    //}
                    DateTime ImsmCalcDate = m_Parameter.HolidayAdjustment.GetIMSMDateFromEffectiveDate(pDtImsm);
                    if (ImsmCalcDate != DateTime.MinValue)
                    {
                        m_DateOfImsm = ImsmCalcDate;
                    }
                    //
                    // Alimentation du Journal
                    m_CalcLogCom.HolydayAdjDays = holydayAdj;
                    m_CalcLogCom.EffectiveImsmDate = m_DateOfImsm;
                }
                //
                m_PreviousImsmDate = m_Exposure.IMSMPreviousDate;
                //
                // ° "Exposure > Minimum"
                // Prendre Exposure de la fenêtre de calcul que si supérieur au minimum (sinon vide)
                m_ExposureMoreThanMin = (
                    from exposure in m_Exposure.WindowExposure
                    where (exposure.Value > m_Parameter.MinIMSM)
                    select new { exposure.Key, exposure.Value }
                    ).ToDictionary(e => e.Key, e => e.Value);
                //
                if (m_Parameter.IsWithHolidayAdjustment)
                {
                    // ° "Holiday adjustment"
                    m_HolidayAdjustment = CalcHolidayAdjustment();
                }
                else
                {
                    // Création d'objets vides pour l'HolidayAdjustment
                    m_OneDayExposureMoreThanMin = new Dictionary<DateTime, decimal>();
                    m_HolidayAdjustment = 0;
                }
                // Alimentation du Journal
                m_CalcLogCom.HolydayAdjAmount = m_HolidayAdjustment;
                //
                // ° T0Exposure > Minimum de la date précédante
                // Si pour un HolidayAdjustment est présent, prendre HolidayAdjustment
                // Sinon prendre T0Exposure à condition qu'il soit supérieur au minimum
                if (0 != m_HolidayAdjustment)
                {
                    // "HolidayAdjustment"
                    m_T0ExposurePreviousDay = m_HolidayAdjustment;
                }
                else
                {
                    // "T0Exposure > Minimum" de la date précédante
                    if (m_Exposure.T0Exposure > m_Parameter.MinIMSM)
                    {
                        m_T0ExposurePreviousDay = m_Exposure.T0Exposure;
                    }
                    else
                    {
                        // PM 20170626 [23261][23257] Ajout remise à 0 de m_T0ExposurePreviousDay
                        m_T0ExposurePreviousDay = 0;
                    }
                }
                // Alimentation du Journal
                m_CalcLogCom.T0Exposure = m_T0ExposurePreviousDay;
                //
                // ° Mean
                // Calcul de la moyenne entre "T0Exposure > Minimum" de la date précédante et les valeurs de "Exposure > Minimum" avant la date précédante
                //
                // "Exposure > Minimum" avant la date précédante
                Dictionary<DateTime, decimal> datapointsValue = m_ExposureMoreThanMin.Where(e => e.Key < m_PreviousImsmDate).ToDictionary(e => e.Key, e => e.Value); ;
                // Ajout de "T0Exposure > Minimum" de la date précédante à l'ensemble des données de statistique
                // RD 20170724 [23345] Add test
                if (0 < m_T0ExposurePreviousDay)
                {
                    // Ne prendre en compte m_T0ExposurePreviousDay que si différent de 0
                    datapointsValue.Add(m_PreviousImsmDate, m_T0ExposurePreviousDay);
                }
                // 
                // ° Mean
                decimal? mean = IMSMTools.Mean(datapointsValue);
                int nbDatapoints = datapointsValue.Count;
                if (mean.HasValue)
                {
                    m_Mean = mean.Value;
                    //
                    // ° Standard Deviation
                    // => Ecart-type en utilisant les moyennes mobiles pondérées exponentielles
                    //
                    // (DatapointsValue - Mean)^2
                    Dictionary<DateTime, decimal> sdNumerator = datapointsValue.ToDictionary(e => e.Key, e => (decimal)System.Math.Pow((double)(e.Value - m_Mean), 2));
                    decimal sumFactor = 0;
                    int pow = 1;
                    // Calcul de la pondération
                    foreach (DateTime key in sdNumerator.Keys.OrderByDescending(e => e))
                    {
                        decimal factor = (decimal)System.Math.Pow((double)m_Parameter.EWMAFactor, pow);
                        sdNumerator[key] *= factor;
                        sumFactor += factor;
                        pow += 1;
                    }
                    if (sumFactor != 0)
                    {
                        // Calcul de l'écart type
                        decimal numerator = sdNumerator.Values.Sum();
                        m_StandardDeviation = (decimal)System.Math.Sqrt((double)(numerator / sumFactor));
                        //
                        // ° Standard Deviation with Securityfactor
                        m_Parameter.SecurityAddon.SecurityAddon.TryGetValue(nbDatapoints, out decimal securityfactor);
                        // Note: Si securityfactor n'est pas trouvé il vaudra 0
                        if (securityfactor <= 0)
                        {
                            securityfactor = 1;
                        }
                        m_SDS = m_StandardDeviation * securityfactor;
                    }
                }
                else
                {
                    m_Mean = 0;
                    m_StandardDeviation = 0;
                    m_SDS = 0;
                }
                // Alimentation du Journal
                m_CalcLogCom.Mean = m_Mean;
                m_CalcLogCom.NoDataPoint = nbDatapoints;
                m_CalcLogCom.StandardDeviation = m_StandardDeviation;
                m_CalcLogCom.SDS = m_SDS;
                //
                // ° Mean + Alpha * SDS
                m_MeanAlphaSDS = m_Mean + m_Parameter.Alpha * m_SDS;
                //
                // ° beta * Max (T-N to T)
                var exposureShortWindow = m_Exposure.ExposureShortWindow(m_DateOfImsm, m_Parameter.WindowSizeMaximum);
                List<decimal> dataForMax = exposureShortWindow.Values.ToList();
                dataForMax.Add(m_T0ExposurePreviousDay);
                decimal max;
                if (dataForMax.Count() > 0)
                {
                    max = dataForMax.Max();
                    m_BetaMax = max * m_Parameter.Beta;
                }
                else
                {
                    max = 0;
                    m_BetaMax = 0;
                }
                // Alimentation du Journal
                m_CalcLogCom.MaxShortWindow = max;
                m_CalcLogCom.BetaMax = m_BetaMax;
                //
                // ° Calcul final du montant de IMSM requis
                decimal maxImsmAmount = System.Math.Max(m_MeanAlphaSDS, m_BetaMax);
                // Alimentation du Journal
                m_CalcLogCom.MainImsm = maxImsmAmount;
                //
                // Arrondi supérieur du montant à 10000 près
                decimal calcImsmAmount = System.Math.Ceiling(maxImsmAmount / 10000) * 10000;
                //
                // PM 20180316 [23840] Ajout gestion en fonction de la version
                if (m_Parameter.MethodVersion > 1)
                {
                    //Ajouter le montant minimum au maximum entre 0 et le montant calculé
                    imsmAmount += System.Math.Max(0, calcImsmAmount);
                }
                else
                {
                    // Prendre le maximum entre le montant minimum et le montant calculé
                    imsmAmount = System.Math.Max(imsmAmount, calcImsmAmount);
                }
                // Alimentation du Journal
                m_CalcLogCom.RoundedImsm = imsmAmount;
                //
            }
            return imsmAmount;
        }

        /// <summary>
        /// Calcul le cas échéant le montant d'ajustement pour jours fériés
        /// </summary>
        /// <returns></returns>
        private decimal CalcHolidayAdjustment()
        {
            decimal holidayAdjustment = 0;
            //
            // ° "Holiday Adjustment"
            // S'il existe un Holiday Adjustement Factor indiquant que le ou les jours suivants sont fériés
            // PM 20231027 [XXXXX] Le Factor Lambda Holiday Adjustment passe de int à decimal
            decimal factorLambda = m_Parameter.HolidayAdjustment.GetAdjustmentFromIMSMDate(m_PreviousImsmDate);
            if (factorLambda > 0)
            {
                // 
                // Prendre Holiday Adjustement Factor (1 ou 2) * moyenne "One day Exposure > Minimum" à partir de la veille
                // + max Exposure à partir du jour pour les WindowMax derniers jour
                //
                // ° "One day Exposure > Minimum"
                // Prendre Exposure supérieur au minimum que si le jour suivant et le jour précédant n'est pas WE (donc si jour in Mardi,Mercredi,Jeudi) (sinon vide)
                // Ne prendre les "One day Exposure > Minimum" qu'à partir de l'avant veille et sur la fenêtre de calcul
                m_OneDayExposureMoreThanMin = (
                    from exposure in m_Exposure.WindowExposure
                    where (exposure.Value > m_Parameter.MinIMSM)
                       && ((exposure.Key.DayOfWeek == DayOfWeek.Tuesday) || (exposure.Key.DayOfWeek == DayOfWeek.Wednesday) || (exposure.Key.DayOfWeek == DayOfWeek.Thursday))
                       && (exposure.Key < m_PreviousImsmDate)
                    select new { exposure.Key, exposure.Value }
                    ).ToDictionary(e => e.Key, e => e.Value);
                //
                // Prendre les "One day Exposure > Minimum" à partir de la veille et sur la fenêtre de calcul
                Dictionary<DateTime, decimal> oneDayExposureFromDayBefore = m_OneDayExposureMoreThanMin.Where(e => (e.Key < m_PreviousImsmDate) && (e.Key >= m_Exposure.WindowDateMin)).ToDictionary(e => e.Key, e => e.Value);
                //
                // D'après le jeu d'essai :
                // Ne pas prendre la plus lointaine "One day Exposure > Minimum" s'il n'existe pas d'Exposure antérieure à celle-ci
                DateTime realMinOnDay = oneDayExposureFromDayBefore.Min(e => e.Key);
                if ((realMinOnDay != m_Exposure.WindowDateMin) && (false == m_Exposure.ExposureDays.Exists(d => d < realMinOnDay)))
                {
                    oneDayExposureFromDayBefore.Remove(realMinOnDay);
                }
                //
                // En calculer la moyenne
                decimal? mean = IMSMTools.Mean(oneDayExposureFromDayBefore);
                if (mean.HasValue)
                {
                    holidayAdjustment = factorLambda * mean.Value;
                }
                // Recherche la date de la borne inférieure pour la fenêtre de calcul maximum de l'Holiday Adjustement
                int decal = m_Parameter.WindowSizeMaximum / 5 * 7;
                DateTime dateMin = m_PreviousImsmDate.AddDays(-1 * decal);
                // Recherche des valeurs Exposure comprises dans la fenêtre de calcul maximum
                IEnumerable<KeyValuePair<DateTime, decimal>> exposureWindow = m_Exposure.WindowExposure.Where(e => (e.Key <= m_PreviousImsmDate) && (e.Key >= dateMin));
                // Recherche de l'Exposure maximum
                decimal maxEposure = exposureWindow.Count() > 0 ? exposureWindow.Max(e => e.Value) : 0;
                // 
                holidayAdjustment += maxEposure;
            }
            return holidayAdjustment;
        }

        /// <summary>
        /// Initialiser les paramètres de calcul
        /// </summary>
        /// <param name="pMethodParameter"></param>
        /// <param name="pHolidayAdjParameter"></param>
        /// <param name="pAddonParameter"></param>
        /// <returns></returns>
        public IMSMGlobalParameterCom SetParameters(ImMethodParameter pMethodParameter,
            IEnumerable<IMSMHolidayAdjustmentParameter> pHolidayAdjParameter,
            IEnumerable<IMSMSecurityAddonParameter> pAddonParameter)
        {
            return m_Parameter.SetParameters(pMethodParameter, pHolidayAdjParameter, pAddonParameter);
        }

        /// <summary>
        /// Initialiser les données statistiques de l'exposition
        /// </summary>
        /// <param name="pExposure"></param>
        /// <returns></returns>
        public IMSMExposureCom SetExposure(IMSMExposure pExposure)
        {
            IMSMExposureCom exposureCom;
            if (pExposure != default(IMSMExposure))
            {
                m_Exposure = pExposure;
                exposureCom = new IMSMExposureCom
                {
                    Exposure = m_Exposure.WindowExposure,
                    T0Exposure = m_Exposure.T0Exposure,
                    WindowDateMin = m_Exposure.WindowDateMin
                };
            }
            else
            {
                exposureCom = default;
            }
            return exposureCom;
        }
        #endregion Methods

        #region Test Methods
        /// <summary>
        /// Calcule du montant d'IMSM pour un jour donnée du jeu de test
        /// </summary>
        /// <param name="pDtImsm"></param>
        /// <returns></returns>
        public decimal Test(DateTime pDtBusiness)
        {
            m_Parameter.SetTestParameter();
            //
            m_Exposure = new IMSMExposure(pDtBusiness, 0, m_Parameter);
            m_Exposure.SetTestExposure();
            //
            return Calc(pDtBusiness, m_Exposure.OlderExposureDate);
        }
        #endregion Test Methods
    }

    /// <summary>
    /// Class pour la Current Exposure Risk Margin pour un Commodity Contract
    /// </summary>
    /// PM 20170808 [23371] New
    internal class IMSMCESMContractExposure
    {
        #region Members
        private decimal? m_MarginParameterBuy = null;
        private decimal? m_MarginParameterSell = null;
        private readonly decimal m_ExposureBuy;
        private readonly decimal m_ExposureSell;
        private readonly int m_IdCC;
        // PM 20200910 [25482] Ajout IdAsset
        private readonly int m_IdAsset;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Identifiant interne du Commodity Contract
        /// </summary>
        public int IdCC
        {
            get { return m_IdCC; }
        }
        /// <summary>
        /// Identifiant interne de l'asset du Commodity Contract
        /// </summary>
        // PM 20200910 [25482] Ajout IdAsset
        public int IdAsset
        {
            get { return m_IdAsset; }
        }
        /// <summary>
        /// Exposure à l'achat
        /// </summary>
        public decimal ExposureBuy
        {
            get { return m_ExposureBuy; }
        }
        /// <summary>
        ///  Exposure à la vente
        /// </summary>
        public decimal ExposureSell
        {
            get { return m_ExposureSell; }
        }
        /// <summary>
        /// Paramètre de risque à l'achat
        /// </summary>
        public decimal? MarginParameterBuy
        {
            get { return m_MarginParameterBuy; }
        }
        /// <summary>
        ///  Paramètre de risque à la vente
        /// </summary>
        public decimal? MarginParameterSell
        {
            get { return m_MarginParameterSell; }
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        // PM 20200910 [25482] Ajout IdAsset
        //public IMSMCESMContractExposure(int pIdCC, decimal pExposureBuy, decimal pExposureSell)
        public IMSMCESMContractExposure(int pIdCC, int pIdAsset, decimal pExposureBuy, decimal pExposureSell)
        {
            m_IdCC = pIdCC;
            m_IdAsset = pIdAsset;
            m_ExposureBuy = pExposureBuy;
            m_ExposureSell = pExposureSell;
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Affectation des margin parameters
        /// </summary>
        /// <param name="pMarginParameterBuy"></param>
        /// <param name="pMarginParameterSell"></param>
        public void SetMarginParameters(decimal? pMarginParameterBuy, decimal? pMarginParameterSell)
        {
            m_MarginParameterBuy = pMarginParameterBuy;
            m_MarginParameterSell = pMarginParameterSell;
        }

        /// <summary>
        /// Calcul du CESM Margin au niveau du commodity contract
        /// </summary>
        /// <returns></returns>
        public decimal RiskMarginValue()
        {
            decimal riskMarginValue;
            if (m_MarginParameterBuy.HasValue || m_MarginParameterSell.HasValue)
            {
                decimal marginParameterBuy = m_MarginParameterBuy ?? 1;
                decimal marginParameterSell = m_MarginParameterSell ?? 1;
                riskMarginValue = (System.Math.Abs(m_ExposureBuy) * marginParameterBuy) + (System.Math.Abs(m_ExposureSell) * marginParameterSell);
            }
            else
            {
                riskMarginValue = (m_ExposureBuy + m_ExposureSell);
            }
            return riskMarginValue;
        }
        #endregion Methods
    }

    /// <summary>
    /// Class pour la Current Exposure Risk Margin
    /// </summary>
    /// PM 20170808 [23371] New
    internal class IMSMCESMCurrentExposure
    {
        #region Members
        // PM 20200910 [25482] Le dictionnaire devient un LookUp
        //private Dictionary<int, IMSMCESMContractExposure> m_CurrentExposure; // Key = IdCC
        private readonly Lookup<int, IMSMCESMContractExposure> m_CurrentExposure; // Key = IdCC
        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient la current exposure
        /// </summary>
        public IEnumerable<IMSMCESMContractExposure> CurrentExposure
        {
            // PM 20200910 [25482] Le dictionnaire devient un LookUp
            //get { return m_CurrentExposure.Values; }
            get
            {
                IEnumerable<IMSMCESMContractExposure> exposure;
                if (m_CurrentExposure != default(Lookup<int, IMSMCESMContractExposure>))
                {
                    exposure =
                        from expCC in m_CurrentExposure
                        from expAsset in expCC
                        select expAsset;
                }
                else
                {
                    exposure = new List<IMSMCESMContractExposure>();
                }
                return exposure;
            }
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pExposure"></param>
        /// <param name="pCESMMarginParameter"></param>
        public IMSMCESMCurrentExposure(IEnumerable<IMSMCESMContractExposure> pExposure, IEnumerable<IMSMCESMMarginParameter> pCESMMarginParameter)
        {
            if (pExposure != default)
            {
                // PM 20200910 [25482] Le dictionnaire devient un LookUp
                //m_CurrentExposure = pExposure.ToDictionary(k => k.IdCC, v => v);
                m_CurrentExposure = (Lookup<int, IMSMCESMContractExposure>)pExposure.ToLookup(k => k.IdCC, k => k);
            }
            else
            {
                // PM 20200910 [25482] Le dictionnaire devient un LookUp
                m_CurrentExposure = default;
            }
            //
            SetParameters(pCESMMarginParameter);
        }
        #endregion Constructor

        #region Methods
        /// <summary>
        /// Initialisation des margin parameters
        /// </summary>
        /// <param name="pCESMMarginParameter"></param>
        private void SetParameters(IEnumerable<IMSMCESMMarginParameter> pCESMMarginParameter)
        {
            if ((pCESMMarginParameter != default)
                && (m_CurrentExposure != default(Lookup<int, IMSMCESMContractExposure>))
                && (pCESMMarginParameter.Count() > 0)
                && (m_CurrentExposure.Count > 0))
            {
                foreach (IMSMCESMMarginParameter param in pCESMMarginParameter)
                {
                    int idCC = param.IdCC;
                    // PM 20200910 [25482] Le dictionnaire devient un LookUp
                    //IMSMCESMContractExposure contractExposure;
                    //if (m_CurrentExposure.TryGetValue(idCC, out contractExposure))
                    //{
                    //    contractExposure.SetMarginParameters(param.MarginParameterBuy, param.MarginParameterSell);
                    //}
                    if (m_CurrentExposure.Contains(idCC))
                    {
                        List<IMSMCESMContractExposure> contractExposureByCC = m_CurrentExposure[idCC].ToList();
                        contractExposureByCC.ForEach(e => e.SetMarginParameters(param.MarginParameterBuy, param.MarginParameterSell));

                    }
                }
            }
        }

        /// <summary>
        /// Calcul du CESM Margin pour la current exposure
        /// </summary>
        /// <returns></returns>
        public decimal RiskMarginValue()
        {
            decimal margin = 0;

            if ((m_CurrentExposure != default(Lookup<int, IMSMCESMContractExposure>))
                && (m_CurrentExposure.Count > 0))
            {
                // PM 20200910 [25482] Le dictionnaire devient un LookUp
                //margin = (
                //    from contract in m_CurrentExposure
                //    select contract.Value.RiskMarginValue()
                //).Sum();
                margin = (
                    from exposure in m_CurrentExposure
                    from contract in exposure
                    select contract.RiskMarginValue()
                    ).Sum();
            }
            return margin;
        }
        #endregion Methods
    }

    /// <summary>
    /// Class de stockage des taux de change
    /// </summary>
    /// PM 20170808 [23371] New
    internal class ExchangeRateParameter
    {
        #region Members
        private readonly FpML.v44.Shared.FxRate m_Rate;
        private readonly bool m_IsAvailable = false;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient le taux de change
        /// </summary>
        public FpML.v44.Shared.FxRate Rate
        { get { return m_Rate; } }
        /// <summary>
        /// Indique si le taux de change est disponible
        /// </summary>
        public bool IsAvailable
        { get { return m_IsAvailable; } }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public ExchangeRateParameter(FpML.v44.Shared.FxRate pRate, bool pIsAvailable)
        {
            m_Rate = pRate;
            m_IsAvailable = pIsAvailable;
        }
        #endregion Constructor
    }

    /// <summary>
    /// Class pour le calcul du Current Exposure Spot Market Risk Margin
    /// </summary>
    /// PM 20170808 [23371] New
    internal class IMSMCESMCalculation
    {
        #region Members
        private IMSMCESMCurrentExposure m_CurrentExposure;
        private IEnumerable<IMSMCESMMarginParameter> m_CESMMarginParameter;
        //
        // Journal du calcul
        private CESMCalculationCom m_CalcLogCom;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient les CESM Margin Parameters
        /// </summary>
        public IEnumerable<IMSMCESMMarginParameter> CESMMarginParameter
        { get { return m_CESMMarginParameter; } }

        /// <summary>
        /// Données du journal du calcul
        /// </summary>
        public CESMCalculationCom CalculationCom
        {
            get { return m_CalcLogCom; }
        }
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public IMSMCESMCalculation()
        {
            m_CESMMarginParameter = default;
        }
        #endregion Constructor

        #region methods
        /// <summary>
        /// Affecte la Current Exposure
        /// </summary>
        /// <param name="pCurrentExposure"></param>
        /// <param name="pAssetCom"></param>
        // PM 20200910 [25482] Ajout paramètre pAssetCom
        //public IMSMCurrentExposureCom[] SetExposure(IMSMCESMCurrentExposure pCurrentExposure)
        public IMSMCurrentExposureCom[] SetExposure(IMSMCESMCurrentExposure pCurrentExposure, Dictionary<int, SQL_AssetCommodityContract> pAssetCom)
           
        {
            if (pCurrentExposure != default(IMSMCESMCurrentExposure))
            {
                m_CurrentExposure = pCurrentExposure;
            }
            else
            {
                m_CurrentExposure = new IMSMCESMCurrentExposure(default, m_CESMMarginParameter);
            }
            // 
            // PM 20200910 [25482] Ajout IdAsset
            //IMSMCurrentExposureCom[] currentExposureCom = (
            //    from exposure in m_CurrentExposure.CurrentExposure
            //     select new IMSMCurrentExposureCom
            //    {
            //        IdCC = exposure.IdCC,
            //        ExposureBuy = exposure.ExposureBuy,
            //        ExposureSell = exposure.ExposureSell,
            //    }).ToArray()
            IMSMCurrentExposureCom[] currentExposureCom = (
                from exposure in m_CurrentExposure.CurrentExposure
                join asset in pAssetCom on exposure.IdAsset equals asset.Key
                select new IMSMCurrentExposureCom
                {
                    IdCC = exposure.IdCC,
                    IdAsset = exposure.IdAsset,
                    AssetIdentifier = asset.Value.Identifier,
                    ExposureBuy = exposure.ExposureBuy,
                    ExposureSell = exposure.ExposureSell,
                }).ToArray();
            return currentExposureCom;
        }

        /// <summary>
        /// Affectation des CESM Margin parameters
        /// </summary>
        /// <param name="pCESMMarginParameter"></param>
        /// <returns></returns>
        public IMSMCESMParameterCom[] SetParameters(IEnumerable<IMSMCESMMarginParameter> pCESMMarginParameter)
        {
            m_CESMMarginParameter = pCESMMarginParameter;
            //
            // Construction de l'objet Com
            IMSMCESMParameterCom[] CESMparameters;
            if (m_CESMMarginParameter != default)
            {
                CESMparameters = (
                    from mp in m_CESMMarginParameter
                    select new IMSMCESMParameterCom
                    {
                        ContractIdentifier = mp.ContractIdentifier,
                        IdCC = mp.IdCC,
                        MarginParameterBuy = mp.MarginParameterBuy,
                        MarginParameterSell = mp.MarginParameterSell,
                    }).ToArray();
            }
            else
            {
                CESMparameters = new IMSMCESMParameterCom[0];
            }
            return CESMparameters;
        }

        // Calcul du Montant de Current Exposure Spot Market Risk Margin
        public decimal Calc()
        {
            // Nouvel objet pour le journal
            m_CalcLogCom = new CESMCalculationCom();
            //
            decimal CESMMargin = 0;
            //
            if (m_CurrentExposure != default(IMSMCESMCurrentExposure))
            {
                CESMMargin = m_CurrentExposure.RiskMarginValue();
            }
            //
            CESMMargin = System.Math.Max(CESMMargin, 0);
            //
            m_CalcLogCom.CESMAMount = CESMMargin;
            //
            return CESMMargin;
        }
        #endregion methods
    }

    /// <summary>
    /// Class principale de la méthode IMSM
    /// </summary>
    public class IMSMMethod : BaseMethod
    {
        #region members
        // PM 20170808 [23371] Ajout m_CS
        private string m_CS = default;
        #region Données de calcul
        private string m_EntityBC = default;
        // PM 20180309 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private IMSMCalculation m_IMSMCalculationMethod = new IMSMCalculation();
        //// PM 20170808 [23371] Ajout m_IMSMCESMCalculationMethod
        //private IMSMCESMCalculation m_IMSMCESMCalculationMethod = new IMSMCESMCalculation();
        private DateTime m_DtBusiness;
        private DateTime m_DtIMSM;
        #endregion Données de calcul
        #region IMSM Parameters
        private IEnumerable<IMSMSecurityAddonParameter> m_SecurityAddonParameters = default;
        private IEnumerable<IMSMHolidayAdjustmentParameter> m_HolidayAdjustmentParameters = default;
        // PM 20170808 [23371] Ajout m_CESMMarginParameters
        private IEnumerable<IMSMCESMMarginParameter> m_CESMMarginParameters = default;
        // PM 20170602 [23212] Ajout m_AgreementDates
        private IEnumerable<IMSMAgreementDate> m_AgreementDates = default;
        // PM 20170808 [23371] Ajout m_CommodityContractParameters
        private IEnumerable<CommodityContractParameter> m_CommodityContractParameters = default;
        // PM 20170808 [23371] Ajout m_ExchangeRateParameters
        // Dictionnaire des taux de change: Key = devise à convertir
        private Dictionary<string, ExchangeRateParameter> m_ExchangeRateParameters = default;
        // PM 20230105 [26219] m_UsedExchangeRateLog est déplacé dans la classe IMSMCalculation
        //// PM 20170808 [23371] Ajout m_UsedExchangeRateLog
        //// Dictionnaire des taux de change réellement utile: Key = devise à convertir
        //private Dictionary<string, IMExchangeRateParameterCom> m_UsedExchangeRateLog = default;
        #endregion IMSM Parameters
        #region CommunicationObject
        // PM 20180309 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private IMSMGlobalParameterCom m_IMSMParameterCom;
        #endregion CommunicationObject
        #endregion members

        #region override base accessors
        /// <summary>
        /// Method Type
        /// </summary>
        public override InitialMarginMethodEnum Type
        {
            get { return InitialMarginMethodEnum.IMSM; }
        }
        #endregion override base accessors

        #region constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        internal IMSMMethod()
        {
            // PM 20170313 [22833] Ajout alimentation de m_RiskMethodDataType
            m_RiskMethodDataType = RiskMethodDataTypeEnum.TradeValue;
        }
        #endregion Constructor

        #region override base methods
        /// <summary>
        /// Charge les paramètres spécifiques à la méthode
        /// </summary>
        /// <param name="pCS">Connection string</param>
        /// <param name="pAssetETDCache">Collection d'assets contenant tous les assets en position</param>
        protected override void LoadSpecificParameters(string pCS, Dictionary<int, SQL_AssetETD> pAssetETDCache)
        {
            Dictionary<string, object> dbParametersValue = new Dictionary<string, object>();
            m_DtBusiness = GetRiskParametersDate(pCS);
            // PM 20170808 [23371] Ajout m_CS
            m_CS = pCS;
            //
            using (IDbConnection connection = DataHelper.OpenConnection(pCS))
            {
                // Set Parameters : DTBUSINESS
                dbParametersValue.Add("DTBUSINESS", m_DtBusiness);

                // SECURITYADDON_IMSMMETHOD
                m_SecurityAddonParameters = LoadParametersMethod<IMSMSecurityAddonParameter>.LoadParameters(connection, dbParametersValue, DataContractResultSets.SECURITYADDON_IMSMMETHOD);

                // CESMPARAMETER_IMSMMETHOD,
                // PM 20170808 [23371] Ajout lecture des Margin Parameter pour le calcul CESM
                m_CESMMarginParameters = LoadParametersMethod<IMSMCESMMarginParameter>.LoadParameters(connection, dbParametersValue, DataContractResultSets.CESMPARAMETER_IMSMMETHOD);

                // ECCSPOTAGREEMENT_IMSMMETHOD
                // PM 20170602 [23212] Ajout lecture agreement
                dbParametersValue.Add("IDA_CSS", IdCSS);
                m_AgreementDates = LoadParametersMethod<IMSMAgreementDate>.LoadParameters(connection, dbParametersValue, DataContractResultSets.ECCSPOTAGREEMENT_IMSMMETHOD);

                // COMMODITYCONTRACTPARAMETER_IMSMMETHOD,
                // PM 20170808 [23371] Ajout lecture des informations sur les Commodity Contracts
                m_CommodityContractParameters = LoadParametersMethod<CommodityContractParameter>.LoadParameters(connection, dbParametersValue, DataContractResultSets.COMMODITYCONTRACTPARAMETER_IMSMMETHOD);

                DateTime dtBusinessFirst = m_DtBusiness.AddDays(-365);
                dbParametersValue.Add("DTBUSINESSFIRST", dtBusinessFirst);

                // HOLIDAYADJUSTMENT_IMSMMETHOD
                m_HolidayAdjustmentParameters = LoadParametersMethod<IMSMHolidayAdjustmentParameter>.LoadParameters(connection, dbParametersValue, DataContractResultSets.HOLIDAYADJUSTMENT_IMSMMETHOD);

                // Lecture des taux de change pouvant être nécessaire
                m_ExchangeRateParameters = LoadExchangeRate();

                // PM 20180309 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
                //// Initialiser les paramètres de la class de calcul
                //m_IMSMParameterCom = m_IMSMCalculationMethod.SetParameters(this.MethodParameter, m_HolidayAdjustmentParameters, m_SecurityAddonParameters);
                //// PM 20170808 [23371] Ajout affectation paramètre m_CESMMarginParameters
                //m_IMSMParameterCom.CESMParameters = m_IMSMCESMCalculationMethod.SetParameters(m_CESMMarginParameters);
            }

            // PM 20170808 [23371] m_DtIMSM est maintenant directement alimenté à partir de DateBusinessNext de EntityMarket dans la méthode BuildMarketParameters
            //IProductBase product = Tools.GetNewProductBase();
            //m_DtIMSM = Tools.ApplyOffset(pCS, product, m_DtBusiness, PeriodEnum.D, 1, DayTypeEnum.ExchangeBusiness, m_EntityBC, BusinessDayConventionEnum.FOLLOWING);
        }

        /// <summary>
        /// Libère les paramètres spécifiques à la méthode
        /// </summary>
        protected override void ResetSpecificParameters()
        {
            m_HolidayAdjustmentParameters = default;
            m_SecurityAddonParameters = default;
            // PM 20170808 [23371] Ajout m_CESMMarginParameters
            m_CESMMarginParameters = default;
            // PM 20170602 [23212] Ajout m_AgreementDates
            m_AgreementDates = default;
            // PM 20170808 [23371] Ajout m_ExchangeRateParameters
            m_ExchangeRateParameters = default;
        }

        /// <summary>
        /// Lecture d'informations complémentaire pour les Marchés/Chambre de compensation utilisant la méthode courante 
        /// </summary>
        /// <param name="pEntityMarkets">La collection de entity/market attaché à la chambre de compensation courante</param>
        public override void BuildMarketParameters(IEnumerable<EntityMarketWithCSS> pEntityMarkets)
        {
            base.BuildMarketParameters(pEntityMarkets);

            EntityMarketWithCSS firstEM = pEntityMarkets.First(elem => elem.CssId == this.IdCSS);

            // Business Center de l'entité
            m_EntityBC = firstEM.EntityBusinessCenter;

            // PM 20170808 [23371] Alimentation de m_DtIMSM directement à partir de DateBusinessNext de EntityMarket
            m_DtIMSM = firstEM.DateBusinessNext;
        }

        /// <summary>
        /// Calcul du montant de déposit pour la position d'un book d'un acteur
        /// </summary>
        /// <param name="pActorId">L'acteur de la position à évaluer</param>
        /// <param name="pBookId">Le book de la position à évaluer</param>
        /// <param name="pDepositHierarchyClass">type de hierarchie pour le couple Actor/Book</param>
        /// <param name="pRiskDataToEvaluate">Données poue lesquelles calculer le déposit</param>
        /// <param name="opMethodComObj">Valeur de retour contenant toutes les données à passer à la feuille de calcul
        /// (<see cref="EFS.SpheresRiskPerformance.CalculationSheet.CalculationSheetRepository"/>) de sorte à construire le noeud
        /// de la méthode de calcul (<see cref="EfsML.v30.MarginRequirement.MarginCalculationMethod"/> 
        /// et <see cref="EfsML.Interface.IMarginCalculationMethod"/>)</param>
        /// <returns>Le montant de déposit correspondant à la position</returns>
        public override List<Money> EvaluateRiskElementSpecific(
            int pActorId, int pBookId, DepositHierarchyClass pDepositHierarchyClass,
            RiskData pRiskDataToEvaluate,
            out IMarginCalculationMethodCommunicationObject opMethodComObj)
        {
            List<Money> riskAmounts = new List<Money>();
            decimal imsmAmount = 0;

            // PM 20180309 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
            IMSMCalculation imsmCalculationMethod = new IMSMCalculation();
            IMSMCESMCalculation imsmCESMCalculationMethod = new IMSMCESMCalculation();
            // Initialiser les paramètres de la class de calcul
            IMSMGlobalParameterCom imsmParameterCom = imsmCalculationMethod.SetParameters(MethodParameter, m_HolidayAdjustmentParameters, m_SecurityAddonParameters);
            imsmParameterCom.CESMParameters = imsmCESMCalculationMethod.SetParameters(m_CESMMarginParameters);

            // Creation de l'objet calculation sheet communication
            IMSMCalcMethCom methodComObj = new IMSMCalcMethCom();
            opMethodComObj = methodComObj;
            methodComObj.MarginMethodType = Type;
            // PM 20180316 [23840] Ajout version
            methodComObj.MethodVersion = MethodParameter.MethodVersion;
            methodComObj.DtParameters = DtRiskParameters;
            methodComObj.CssCurrency = m_CssCurrency;
            methodComObj.DtIMSM = m_DtIMSM;
            methodComObj.BusinessCenter = m_EntityBC;
            methodComObj.IMSMParameter = imsmParameterCom;

            // PM 20230105 [26219] Déplacé dans imsmCalculationMethod
            //// PM 20170808 [23371] Ajout m_UsedExchangeRateLog
            //m_UsedExchangeRateLog = new Dictionary<string, IMExchangeRateParameterCom>();

            // Lancement du calcul
            if ((pRiskDataToEvaluate != default(RiskData)) && (pRiskDataToEvaluate.TradeValueCOM.Count() > 0))
            {
                // PM 20170808 [23371] Construction de la currentExposure
                // PM 20180309 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
                //IMSMCESMCurrentExposure currentExposure = BuildCESMContractExposure(pRiskDataToEvaluate);
                // PM 20230105 [26219] Correction problème multithreading
                //IMSMCESMCurrentExposure currentExposure = BuildCESMContractExposure(imsmCESMCalculationMethod.CESMMarginParameter, pRiskDataToEvaluate);
                IMSMCESMCurrentExposure currentExposure = BuildCESMContractExposure(imsmCESMCalculationMethod.CESMMarginParameter, pRiskDataToEvaluate, imsmCalculationMethod.UsedExchangeRateLog);
                methodComObj.CurrentExposure = imsmCESMCalculationMethod.SetExposure(currentExposure, pRiskDataToEvaluate.AssetCOMCache);
                //
                // PM 20230105 [26219] Correction problème multithreading: déplacé dans imsmCalculationMethod
                // PM 20170808 [23371] Ajout log ExchangeRate
                //methodComObj.ExchangeRate = m_UsedExchangeRateLog.Values.ToArray();
                //
                // PM 20200910 [25482] Si pas de calcul uniquement du CESM
                if (false == MethodParameter.IsCalcCESMOnly)
                {
                    // 
                    // PM 20180309 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
                    //IMSMExposure exposure = BuildExposure(pRiskDataToEvaluate);
                    // PM 20230105 [26219] Correction problème multithreading
                    //IMSMExposure exposure = BuildExposure(imsmCalculationMethod.Parameter, pRiskDataToEvaluate);
                    IMSMExposure exposure = BuildExposure(imsmCalculationMethod, pRiskDataToEvaluate);
                    methodComObj.Exposure = imsmCalculationMethod.SetExposure(exposure);
                    //
                    // PM 20170602 [23212] Utilisation de la date d'agrément à la place de la date du premier trade.
                    //imsmAmount = m_IMSMCalculationMethod.Calc(m_DtIMSM, pRiskDataToEvaluate.TradeValueCOM.GetMinFirstTradeDate());
                    DateTime agreementDate = DateTime.MinValue;
                    if (m_AgreementDates != default)
                    {
                        var dates = m_AgreementDates.Where(a => (a.IdA == pActorId)).Select(a => a.DtAgreement);
                        if (dates.Count() > 0)
                        {
                            agreementDate = dates.Min();
                        }
                    }
                    imsmAmount = imsmCalculationMethod.Calc(m_DtIMSM, agreementDate);
                    //
                    methodComObj.IMSMCalculationData = imsmCalculationMethod.LastCalculationCom;
                    //
                }
                //
                // PM 20170808 [23371] Calcul du montant de CESM Margin
                decimal cesmAmount = imsmCESMCalculationMethod.Calc();
                //
                methodComObj.CESMCalculationData = imsmCESMCalculationMethod.CalculationCom;
                //
                imsmAmount += cesmAmount;
                //
                // PM 20230105 [26219] Alimentation du log ExchangeRate
                methodComObj.ExchangeRate = imsmCalculationMethod.UsedExchangeRateLog.Values.ToArray();
            }

            //#if DEBUG
            //            // Pour effectuer un test de calcul
            //            IMSMCalculation test = new IMSMCalculation();
            //            test.Test(new DateTime(2015, 12, 28));
            //#endif

            if (StrFunc.IsEmpty(methodComObj.CssCurrency))
            {
                riskAmounts.Add(new Money(imsmAmount, "EUR"));
            }
            else
            {
                riskAmounts.Add(new Money(imsmAmount, methodComObj.CssCurrency));
            }
            return riskAmounts;
        }

        /// <summary>
        /// Get a collection of sorting parameter needed by coverage strategies
        /// </summary>
        /// <param name="pGroupedPositionsByIdAsset">Positions of the current risk element</param>
        /// <returns>A collection of sorting parameters in order to be used inside of the ReducePosition method </returns>
        protected override IEnumerable<CoverageSortParameters> GetSortParametersForCoverage(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset)
        {
            return null;
        }
        #endregion override base methods

        #region methods
        /// <summary>
        /// Construction des données de l'exposition à partir des données de risque reçues par la méthode
        /// </summary>
        /// <param name="pIMSMParameter"></param>
        /// <param name="pIMSMCalculationData"></param>
        /// <returns></returns>
        // PM 20180309 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private IMSMExposure BuildExposure(RiskData pRiskDataToEvaluate)
        // PM 20230105 [26219] Correction problème multithreading
        //private IMSMExposure BuildExposure(IMSMParameter pIMSMParameter, RiskData pRiskDataToEvaluate)
        private IMSMExposure BuildExposure(IMSMCalculation pIMSMCalculationData, RiskData pRiskDataToEvaluate)
        {
            IMSMExposure exposure;
            if ((pRiskDataToEvaluate != default(RiskData)) && (pIMSMCalculationData != default(IMSMCalculation)))
            {
                // PM 20170808 [23371] Ajout conversion
                //decimal t0Exposure = pRiskDataToEvaluate.TradeValueCOM.GetSumValueOfDate(m_DtBusiness);
                decimal t0Exposure = SumAndConvert(pRiskDataToEvaluate.TradeValueCOM.GetSumValueOfDate(m_DtBusiness), pIMSMCalculationData.UsedExchangeRateLog);
                // PM 20180309 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
                //exposure = new IMSMExposure(m_DtIMSM, t0Exposure, m_IMSMCalculationMethod.Parameter);
                exposure = new IMSMExposure(m_DtIMSM, t0Exposure, pIMSMCalculationData.Parameter);

                // PM 20170808 [23371] Ajout conversion
                //IEnumerable<KeyValuePair<DateTime, decimal>> valueDic = pRiskDataToEvaluate.TradeValueCOM.GetSumValueByDate();
                Dictionary<DateTime, IEnumerable<IMoney>> valueDicAllCurrencies = pRiskDataToEvaluate.TradeValueCOM.GetSumValueByDate();
                IEnumerable<KeyValuePair<DateTime, decimal>> valueDic =
                    from dic in valueDicAllCurrencies
                    select new KeyValuePair<DateTime, decimal>(dic.Key, SumAndConvert(dic.Value, pIMSMCalculationData.UsedExchangeRateLog));
                valueDic = valueDic.Where(k => k.Key < m_DtBusiness && k.Key >= exposure.WindowDateMin);
                //
                exposure.Add(valueDic);
            }
            else
            {
                // PM 20180309 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
                //exposure = new IMSMExposure(m_DtIMSM, 0, m_IMSMCalculationMethod.Parameter);
                exposure = new IMSMExposure(m_DtIMSM, 0, pIMSMCalculationData.Parameter);
            }
            return exposure;
        }

        /// <summary>
        /// Construit la current exposure par contract
        /// </summary>
        /// <param name="pCESMMarginParameter"></param>
        /// <param name="pRiskDataToEvaluate"></param>
        /// <param name="pUsedExchangeRateLog"></param>
        /// <returns></returns>
        // PM 20170808 [23371] New
        // PM 20180309 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
        //private IMSMCESMCurrentExposure BuildCESMContractExposure(RiskData pRiskDataToEvaluate)
        // PM 20230105 [26219] Ajout pUsedExchangeRateLog pour correction problème multithreading
        //private IMSMCESMCurrentExposure BuildCESMContractExposure(IEnumerable<IMSMCESMMarginParameter> pCESMMarginParameter, RiskData pRiskDataToEvaluate)
        private IMSMCESMCurrentExposure BuildCESMContractExposure(IEnumerable<IMSMCESMMarginParameter> pCESMMarginParameter, RiskData pRiskDataToEvaluate, Dictionary<string, IMExchangeRateParameterCom> pUsedExchangeRateLog)

        {
            IMSMCESMCurrentExposure currentExposure;
            IEnumerable<IMSMCESMContractExposure> contractExposure;
            if ((pRiskDataToEvaluate != default(RiskData)) && (pCESMMarginParameter != default) && (pUsedExchangeRateLog != default))
            {
                IEnumerable<RiskMarginTradeValue> riskMarginTradeValue = pRiskDataToEvaluate.TradeValueCOM.GetRiskMarginTradeValueOfDate(m_DtIMSM);
                riskMarginTradeValue = riskMarginTradeValue.Where(t => t.Trades != default);
                
                IEnumerable<TradeValue> allTrades =
                    from trades in riskMarginTradeValue
                    from trade in trades.Trades
                    select trade;

                // PM 20200910 [25482] Ajout IdAsset
                //var tradeCC =
                //    from trade in allTrades
                //    group trade by new { trade.IdCC, trade.Currency, trade.IdAsset }
                //    into tradeByCC
                //    select new
                //    {
                //        IdCC = tradeByCC.Key.IdCC,
                //        ValueBuy = SumAndConvert(tradeByCC.Key.Currency, tradeByCC.Where(v => v.Value > 0).Select(v => v.Value)),
                //        ValueSell = SumAndConvert(tradeByCC.Key.Currency, tradeByCC.Where(v => v.Value < 0).Select(v => v.Value)),
                //    };

                //contractExposure =
                //    from trade in tradeCC
                //    group trade by trade.IdCC
                //    into tradeByCC
                //    select new IMSMCESMContractExposure(tradeByCC.Key, tradeByCC.Sum(v => v.ValueBuy), tradeByCC.Sum(v => v.ValueSell));

                var tradeAsset =
                    from trade in allTrades
                    group trade by new { trade.IdCC, trade.Currency, trade.IdAsset}
                    into tradeByAsset
                    select new
                    {
                        tradeByAsset.Key.IdAsset,
                        tradeByAsset.Key.IdCC,
                        ValueBuy = SumAndConvert(tradeByAsset.Key.Currency, tradeByAsset.Where(v => v.Value > 0).Select(v => v.Value), pUsedExchangeRateLog),
                        ValueSell = SumAndConvert(tradeByAsset.Key.Currency, tradeByAsset.Where(v => v.Value < 0).Select(v => v.Value), pUsedExchangeRateLog),
                    };

                contractExposure =
                    from trade in tradeAsset
                    group trade by new { trade.IdCC, trade.IdAsset }
                    into tradeByAsset
                    select new IMSMCESMContractExposure(tradeByAsset.Key.IdCC, tradeByAsset.Key.IdAsset, tradeByAsset.Sum(v => v.ValueBuy), tradeByAsset.Sum(v => v.ValueSell));
            }
            else
            {
                contractExposure = default;
            }
            // PM 20180309 [CHEETAH] Rendre les données d'évaluation propre à chaque appel du calcul
            //currentExposure = new IMSMCESMCurrentExposure(contractExposure, m_IMSMCESMCalculationMethod.CESMMarginParameter);
            currentExposure = new IMSMCESMCurrentExposure(contractExposure, pCESMMarginParameter);
            return currentExposure;
        }

        /// <summary>
        /// Charge les différents taux de change pouvant être nécessaire pour les conversions de devise
        /// </summary>
        /// <returns></returns>
        /// PM 20170808 [23371] New
        private Dictionary<string, ExchangeRateParameter> LoadExchangeRate()
        {
            Dictionary<string, ExchangeRateParameter> exchangeRateParameters = new Dictionary<string, ExchangeRateParameter>();
            IEnumerable<string> allCurrencies = m_CommodityContractParameters.Where(cc => cc.Currency != m_CssCurrency).Select(cc => cc.Currency).Distinct();
            if (allCurrencies.Count() > 0)
            {
                IProductBase productBase = Tools.GetNewProductBase();
                foreach (string currrency in allCurrencies)
                {
                    if (currrency != m_CssCurrency)
                    {
                        KeyQuote keyQuote = new KeyQuote(m_CS, m_DtBusiness, null, null, FpML.Enum.QuotationSideEnum.OfficialClose, QuoteTimingEnum.Close);

                        KeyAssetFxRate keyAssetFXRate = new KeyAssetFxRate
                        {
                            IdC1 = currrency,
                            IdC2 = m_CssCurrency
                        };
                        keyAssetFXRate.SetQuoteBasis(true);

                        SQL_Quote quote = new SQL_Quote(m_CS, QuoteEnum.FXRATE, AvailabilityEnum.Enabled, productBase, keyQuote, keyAssetFXRate);

                        FpML.v44.Shared.FxRate retRate = new FpML.v44.Shared.FxRate
                        {
                            quotedCurrencyPair = new QuotedCurrencyPair(keyAssetFXRate.IdC1, keyAssetFXRate.IdC2, ((KeyAssetFxRate)quote.KeyAssetIN).QuoteBasis)
                        };

                        bool isOk = quote.IsLoaded;
                        if (isOk && (quote.QuoteValueCodeReturn == Cst.ErrLevel.SUCCESS))
                        {
                            retRate.rate = new EFS_Decimal(quote.QuoteValue);
                        }
                        else
                        {
                            retRate.rate = new EFS_Decimal(1);
                            isOk = false;
                        }
                        exchangeRateParameters.Add(currrency, new ExchangeRateParameter(retRate, isOk));
                    }
                }
            }
            return exchangeRateParameters;
        }

        /// <summary>
        /// Convertie un enssemble de IMoney dans la devise de calcul du déposit et retourne la somme des ces montants
        /// </summary>
        /// <param name="pMoney"></param>
        /// <param name="pUsedExchangeRateLog"></param>
        /// <returns></returns>
        // PM 20170808 [23371] New
        // PM 20230105 [26219] Ajout pUsedExchangeRateLog pour correction problème multithreading
        //private decimal SumAndConvert(IEnumerable<IMoney> pMoney)
        private decimal SumAndConvert(IEnumerable<IMoney> pMoney, Dictionary<string, IMExchangeRateParameterCom> pUsedExchangeRateLog)
        {
            decimal amountSum = 0;
            if ((pMoney != default(IEnumerable<IMoney>)) && (pMoney.Count() > 0) && (pUsedExchangeRateLog != default))
            {
                foreach (IMoney money in pMoney)
                {
                    decimal amount = money.Amount.DecValue;
                    if (money.Currency != m_CssCurrency)
                    {
                        // Conversion nécessaire si devise différente de celle du déposit
                        if (m_ExchangeRateParameters.TryGetValue(money.Currency, out ExchangeRateParameter exchangeRate))
                        {
                            if (exchangeRate.IsAvailable)
                            {
                                EFS_Cash cash = new EFS_Cash(m_CS,
                                    exchangeRate.Rate.quotedCurrencyPair.currency1.Value,
                                    exchangeRate.Rate.quotedCurrencyPair.currency2.Value,
                                    amount,
                                    exchangeRate.Rate.rate.DecValue,
                                    exchangeRate.Rate.quotedCurrencyPair.quoteBasis);
                                //
                                amount = cash.ExchangeAmountRounded;
                            }
                            // Log
                            if (false == pUsedExchangeRateLog.TryGetValue(money.Currency, out IMExchangeRateParameterCom exchangeRateLog))
                            {
                                exchangeRateLog = new IMExchangeRateParameterCom(
                                    exchangeRate.Rate.quotedCurrencyPair.currency1.Value,
                                    exchangeRate.Rate.quotedCurrencyPair.currency2.Value,
                                    exchangeRate.Rate.quotedCurrencyPair.quoteBasis,
                                    exchangeRate.Rate.rate.DecValue,
                                    false == exchangeRate.IsAvailable);
                                //
                                pUsedExchangeRateLog.Add(money.Currency, exchangeRateLog);
                            }
                        }
                    }
                    amountSum += amount;
                }
            }
            return amountSum;
        }

        /// <summary>
        /// Convertie un enssemble de montant dans la devise de calcul du déposit et retourne la somme des ces montants
        /// </summary>
        /// <param name="pCurrency"></param>
        /// <param name="pAmount"></param>
        /// <param name="pUsedExchangeRateLog"></param>
        /// <returns></returns>
        // PM 20170808 [23371] New
        // PM 20230105 [26219] Ajout pUsedExchangeRateLog pour correction problème multithreading
        //private decimal SumAndConvert(string pCurrency, IEnumerable<decimal> pAmount)
        private decimal SumAndConvert(string pCurrency, IEnumerable<decimal> pAmount, Dictionary<string, IMExchangeRateParameterCom> pUsedExchangeRateLog)
        {
            decimal amountSum = 0;
            if ((pAmount != default(IEnumerable<IMoney>)) && (pAmount.Count() > 0) && (pUsedExchangeRateLog != default))
            {
                amountSum = pAmount.Sum();
                if (pCurrency != m_CssCurrency)
                {
                    // Conversion nécessaire si devise différente de celle du déposit
                    if (m_ExchangeRateParameters.TryGetValue(pCurrency, out ExchangeRateParameter exchangeRate))
                    {
                        if (exchangeRate.IsAvailable)
                        {
                            EFS_Cash cash = new EFS_Cash(m_CS,
                                exchangeRate.Rate.quotedCurrencyPair.currency1.Value,
                                exchangeRate.Rate.quotedCurrencyPair.currency2.Value,
                                amountSum,
                                exchangeRate.Rate.rate.DecValue,
                                exchangeRate.Rate.quotedCurrencyPair.quoteBasis);

                            amountSum = cash.ExchangeAmountRounded;
                        }
                        // Log
                        if (false == pUsedExchangeRateLog.TryGetValue(pCurrency, out _))
                        {
                            IMExchangeRateParameterCom exchangeRateLog = new IMExchangeRateParameterCom(
                                exchangeRate.Rate.quotedCurrencyPair.currency1.Value,
                                exchangeRate.Rate.quotedCurrencyPair.currency2.Value,
                                exchangeRate.Rate.quotedCurrencyPair.quoteBasis,
                                exchangeRate.Rate.rate.DecValue,
                                false == exchangeRate.IsAvailable);

                            pUsedExchangeRateLog.Add(pCurrency, exchangeRateLog);
                        }
                    }
                }
            }
            return amountSum;
        }
        #endregion methods
    }
}