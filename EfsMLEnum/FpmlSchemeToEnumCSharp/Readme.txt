// FI 20171005 
Permet de bâtir le l'enum c# à partir d'une source Fpml

1/ ouvrir l'URL avec studio
Ex http://www.fpml.org/coding-scheme/account-type-1-1.xml

2/ Appliquer la transformation XSLT

3/ On obtient
/// <summary >
/// Contains a code representing the type of an account, for example in a clearing or exchange model.
/// </summary>
public enum accountTypeScheme
{
/// <summary >
/// Aggregate client account, as defined under ESMA MiFIR.
/// </summary>
AggregateClient,
/// <summary >
/// The account contains trading activity or positions that belong to a client of the firm that opened the account.
/// </summary>
Client,
/// <summary >
/// The account contains proprietary trading activity or positions, belonging to the firm that is the owner of the account.
/// </summary>
House,
}
