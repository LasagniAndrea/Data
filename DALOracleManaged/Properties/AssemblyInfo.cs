﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Les informations générales relatives à un assembly dépendent de
// l'ensemble d'attributs suivant. Changez les valeurs de ces attributs pour modifier les informations
// associées à un assembly.
[assembly: AssemblyTitle("DALOracle.Managed")]
[assembly: AssemblyDescription("ODP.NET, Managed Driver for Oracle Database")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Euro Finance Systems")]
[assembly: AssemblyProduct("Spheres")]
[assembly: AssemblyCopyright("© 2024 EFS")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

//
// Les informations de version pour un assembly se composent des quatre valeurs suivantes :
//
//      Version principale
//      Version secondaire 
//      Numéro de build
//      Révision
//
// Vous pouvez spécifier toutes les valeurs ou indiquer des numéros de révision et de build par défaut 
// en utilisant '*', comme ci-dessous :

[assembly: AssemblyVersion("14.0.*")]


// L'affectation de la valeur false à ComVisible rend les types invisibles dans cet assembly
// aux composants COM. Si vous devez accéder à un type dans cet assembly à partir de
// COM, affectez la valeur true à l'attribut ComVisible sur ce type.
[assembly: ComVisible(false)]

// Le GUID suivant est pour l'ID de la typelib si ce projet est exposé à COM
[assembly: Guid("dafcddc0-fbec-4776-9fe9-61ceb806e9d4")]

//
// Pour signer votre assembly, vous devez spécifier la clé à utiliser. Consultez 
// la documentation Microsoft .NET Framework pour plus d'informations sur la signature d'un assembly.
//
// Utilisez les attributs ci-dessous pour contrôler la clé utilisée lors de la signature. 
//
// Remarques : 
//   (*) Si aucune clé n'est spécifiée, l'assembly n'est pas signé.
//   (*) KeyName fait référence à une clé installée dans le fournisseur de
//       services cryptographiques (CSP) de votre ordinateur. KeyFile fait référence à un fichier qui contient
//       une clé.
//   (*) Si les valeurs de KeyFile et de KeyName sont spécifiées, le 
//       traitement suivant se produit :
//       (1) Si KeyName se trouve dans le CSP, la clé est utilisée.
//       (2) Si KeyName n'existe pas mais que KeyFile existe, la clé 
//           de KeyFile est installée dans le CSP et utilisée.
//   (*) Pour créer KeyFile, vous pouvez utiliser l'utilitaire sn.exe (Strong Name, Nom fort).
//        Lors de la spécification de KeyFile, son emplacement doit être
//        relatif au répertoire de sortie du projet qui est
//       %Project Directory%\obj\<configuration>. Par exemple, si votre KeyFile se trouve
//       dans le répertoire du projet, vous devez spécifier l'attribut 
//       AssemblyKeyFile sous la forme [assembly: AssemblyKeyFile("..\\..\\mykey.snk")]
//   (*) DelaySign (signature différée) est une option avancée. Pour plus d'informations, consultez la
//       documentation Microsoft .NET Framework.
//
[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile("")]
[assembly: AssemblyKeyName("")]
