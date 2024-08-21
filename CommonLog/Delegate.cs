using System;

namespace EFS.Common.Log
{

    /// <summary>
    ///  Retourne l'horodatage courant sur le SGBD (fuseau horaire du serveur SQL)
    /// </summary>
    /// <param name="pCS">ConnectionString d'accès SGBD</param>
    /// <returns></returns>
    /// FI 20200812 [XXXXX] Add
    public delegate DateTime GetDateSys(string pCS);

    /// <summary>
    ///  Retourne l'horodatage courant sur le SGBD (fuseau horaire UTC)
    /// </summary>
    /// <param name="pCS">ConnectionString d'accès SGBD</param>
    /// <returns></returns>
    /// FI 20200812 [XXXXX] Add
    public delegate DateTime GetDateSysUTC(string pCS);
}
