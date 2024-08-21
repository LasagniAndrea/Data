<?xml version="1.0" encoding="utf-8"?>
<!-- 
=========================================================================================================
 Summary   : XSL commun aux éléments "ICE-EU - Repository - Liffe" &
                                     "ICE-EU - Repository - Liffe Equity"
 File      : MarketData_LiffeCommon_Import_Map.xsl
=========================================================================================================
Version : v8.1.8040
  Date    : 20220105
	Author  : FL/RD
	Comment : 25920 - ICE-EU - Repository error : Row is rejected by controls. [EFS]
  	  - Management in templates: GetSettltMethod_LIFFE, GetAssetCategory_LIFFE, GetUnderlyingAsset_LIFFE
         of all DCs of type: Eris EURIBOR & Eris GBP SONIA

		     Eris EURIBOR
			      6-month, IMM:  de 2F à 2O ( de 1 year à 10 year)
			      6-month, IMM:  2S  (30 year)
			      3-month, IMM: de 3F à 3O (de 1 year à 10 year)
            https://www.theice.com/products/79894236/Eris-EURIBOR-10YR-IMM-6M-Interest-Rate-Future
      
		     Eris GBP SONIA
			      6-month, IMM: 6F - 6O ( de 1 year - 10 year)
			      6-month, IMM: 6S ( 30 year)
            https://www.theice.com/products/79894238/Eris-GBP-3YR-Interest-Rate-Future

        Ps. By managing this list of DCs a lot of DCs managed in Static have been deleted in the templates,
            because they are included in these new rules put in place.

      - Add parameter pIsCheckAssetCategory and display Warning if Asset category is missing
         on SQLTableDERIVATIVECONTRACT template
    
      - Manage default value "EquityAsset" only for Exchange symbol "O" on GetAssetCategory_LIFFE template
=========================================================================================================         
Version : v8.1.7943
  Date    : 20210930
	Author  : FL
	Comment : 25863 - ICE-EU - Repository error : Row is rejected by controls. [EFS]
  	  - Management in templates: GetSettltMethod_LIFFE, GetAssetCategory_LIFFE, GetUnderlyingAsset_LIFFE
         Derivatives Contracts with Symbol: 
            2OF - Eris EURIBOR 10YR
                https://www.theice.com/products/79894233/Eris-EURIBOR-3YR-IMM-6M-Interest-Rate-Future
                => 6-month, IMM: 2F - 2O (1 year - 10 year)
            2SG - Eris EURIBOR 30YR
                https://www.theice.com/products/79894237/Eris-EURIBOR-30YR-IMM-6M-Interest-Rate-Future
            6HG -Eris GBP SONIA 3GBP
                https://www.theice.com/products/79894238/Eris-GBP-3YR-Interest-Rate-Future
=========================================================================================================         
Version : v8.1.7745
  Date    : 20210625
	Author  : FL
	Comment : 25794 - ICE-EU - Repository error : Row is rejected by controls. [EFS]
  	  - Management in templates: GetSettltMethod_LIFFE, GetAssetCategory_LIFFE, GetUnderlyingAsset_LIFFE
         Derivatives Contracts with Symbol: 
            2HD - Eris EURIBOR 3YR Futures, 2JD - Eris EURIBOR 5YR Futures, 2LE - Eris EURIBOR 7YR Futures, 2OE - Eris EURIBOR 10YR Futures
                https://www.theice.com/products/79894233/Eris-EURIBOR-3YR-IMM-6M-Interest-Rate-Future
                => 6-month, IMM: 2F - 2O (1 year - 10 year)
            2SF - Eris EURIBOR 30YR Futures
                https://www.theice.com/products/79894237/Eris-EURIBOR-30YR-IMM-6M-Interest-Rate-Future
            6HF - Eris GBP SONIA 3GBP Futures
                https://www.theice.com/products/79894238/Eris-GBP-3YR-Interest-Rate-Future
            6JG - Eris GBP SONIA 5GBP Futures
                https://www.theice.com/products/79894239/Eris-GBP-5YR-Interest-Rate-Future
            6LH - Eris GBP SONIA 7GBP Futures
                No Found
            6OH - Eris GBP SONIA 1GBP Futures
                https://www.theice.com/products/79894243/Eris-GBP-10YR-Interest-Rate-Future
            6SI - Eris GBP SONIA 3GBP Futures
                https://www.theice.com/products/79894244/Eris-GBP-30YR-Interest-Rate-Future
=========================================================================================================         
Version : v8.1.7745
  Date    : 20210316
	Author  : FL
	Comment : 25678 - ICE-EU - Repository error : Row is rejected by controls. [EFS]
  	  - Management in templates: GetSettltMethod_LIFFE, GetAssetCategory_LIFFE, GetUnderlyingAsset_LIFFE
         Derivatives Contracts with Symbol: 
              6GF - Eris GBP SONIA 2 Futures
                  https://www.theice.com/products/52026300/Eris-GBP-2YR-Interest-Rate-Future
              6GG - Eris GBP SONIA 2GBP Futures
                  https://www.theice.com/products/?filter=Eris
              6GH - Eris GBP SONIA 2GBP Futures
                  https://www.theice.com/products/?filter=Eris
              6GK - Eris GBP SONIA 2GBP Futures
                  https://www.theice.com/products/?filter=Eris
              6GO - Eris GBP SONIA 2 Futures
                  https://www.theice.com/products/?filter=Eris
              6FF - Eris GBP SONIA 1 Futures
                  https://www.theice.com/products/?filter=Eris
=========================================================================================================         
Version : v8.1.7656
  Date    : 20201217
	Author  : FL
	Comment : 25606 - ICE-EU - Repository error : Row is rejected by controls. [EFS]
  	  - Management in templates: GetSettltMethod_LIFFE, GetAssetCategory_LIFFE, GetUnderlyingAsset_LIFFE
         Derivatives Contracts with Symbol: 
              SO3 (Options on Three Month SONIA Index Future)
	                 https://www.theice.com/products/79341513/Options-on-Three-Month-SONIA-Index-Future
              SY1 (Options on One-Year-Mid-Curve-Option-on-Three-Month-SONIA-Index-Future)
	                 https://www.theice.com/products/79341516/One-Year-Mid-Curve-Option-on-Three-Month-SONIA-Index-Future
              SY2 (Options on Two Year Mid-Curve Option on Three Month SONIA Index Future)
	                 https://www.theice.com/products/79341532/Two-Year-Mid-Curve-Option-on-Three-Month-SONIA-Index-Future
              SY3 (Options on Three Year Mid-Curve Option on Three Month SONIA Index Future)
	                 https://www.theice.com/products/79341534/Three-Year-Mid-Curve-Option-on-Three-Month-SONIA-Index-Future
              SY4 (Options on Four Year Mid-Curve Option on Three Month SONIA Index Future)
	                 https://www.theice.com/products/79341536/Four-Year-Mid-Curve-Option-on-Three-Month-SONIA-Index-Future
              SY5 (Options on Five Year Mid-Curve Option on Three Month SONIA Index Future)
	                 https://www.theice.com/products/79341538/Five-Year-Mid-Curve-Option-on-Three-Month-SONIA-Index-Future
                   
      - Management in template: ovrSQLGetDerivativeContractDefaultValue Options on Futures Mide-Curve SY1,SY2,SY3
         SY4,SY5 whose Underlying Futures Contract is the SO3
=========================================================================================================
Version : v8.0.7156
  Date    : 20190805
	Author  : PLA
	Comment : 24824 - ICE-EU - Repository error : Row is rejected by controls. [EFS]
  	  - Management in templates : GetSettltMethod_LIFFE, GetAssetCategory_LIFFE, GetUnderlyingAsset_LIFFE
         Derivatives Contracts with Symbol: 
              SA3 (Three Month SAROCHF Index Futures) 
                   https://www.theice.com/products/72270612
=========================================================================================================
Version : v7.2.6828
  Date    : 20181024
	Author  : PLA
	Comment : 24273 - ICE-EU - Repository error : Row is rejected by controls. [EFS]
  	  - Management in templates: GetSettltMethod_LIFFE, GetAssetCategory_LIFFE, GetUnderlyingAsset_LIFFE
         Derivatives Contracts with Symbol: 
              SF1 (One Month SOFR Index Futures) 
                   https://www.theice.com/products/70005423/ICE-Futures-Europe-One-Month-SOFR-Index-Futures-Contract-Specification
              SF3 (Three Month SOFR Index Futures) 
                   https://www.theice.com/products/70005442/ICE-Futures-Europe-Three-Month-SOFR-Index-Futures-Contract-Specification
=========================================================================================================
Version : v7.0.6729
  Date    : 20180604
	Author  : FL
	Comment : 23999 - ICE-EU - Repository error : Row is rejected by controls. [EFS]
  	  - Management in templates: GetSettltMethod_LIFFE, GetAssetCategory_LIFFE, GetUnderlyingAsset_LIFFE
         Derivatives Contracts with Symbol:         
            SO3 (Three Month SONIA Index Future)
	                 https://www.theice.com/products/68361266/Three-Month-SONIA-Index-Futures 
=========================================================================================================         
Version : v6.0.6548
  Date    : 20171205
	Author  : FL
	Comment : 23615 - ICE-EU - Repository error : Row is rejected by controls. [EFS]
  	  - Management in templates: GetSettltMethod_LIFFE, GetAssetCategory_LIFFE, GetUnderlyingAsset_LIFFE
         Derivatives Contracts with Symbol: SOA (One Month SONIA Index Futures)
=========================================================================================================
 Version : v6.0.0.0    
   Date    : 20170420    
   Author  : FL/PLA
   Comment : [23064] - Derivative Contracts: Settled amount behavior for "Physical" delivery
   Add pPhysettltamount parameter on SQLTableDERIVATIVECONTRACT template
=========================================================================================================
Version : v5.1.6241
	Date    : 20170322 (Report en 6.0 d'une modification fait en 5.1 le 20170201)
	Author  : FL
	Comment : 22702 - ICE-EU - Repository error : Row is rejected by controls. [EFS]
  	  - Management in templates: GetSettltMethod_LIFFE, GetAssetCategory_LIFFE, GetUnderlyingAsset_LIFFE
         DCs whose Contract Symbol begins with the letters 'R', 'Q', 'P', 'V' with a description
         containing 'Eris' allowing to manage all DCs of type:
          - Eris GBP LIBOR® Interest Rate Future ..............
          - Eris EURIBOR® Interest Rate Future   ..............
      - Template: GetSettltMethod_LIFFE
          New Parameter : pContractDescription
    Ps. This modification replaces the modifications in this XSL made since 07/03/2015 for
           manage DCs of type:
             - Eris GBP LIBOR Interest Rate Future
             - Eris EURIBOR® Interest Rate Future
           and is valid for all new DCs that will be created on the LIFFE market (IFLL) of the same type.
=========================================================================================================
Version : v5.1.6204
	Date    : 20161226
	Author  : FL&PLA
	Comment : 22702 - ICE-EU - Repository error : Row is rejected by controls. [EFS]
  	  - Management in templates: GetSettltMethod_LIFFE, GetAssetCategory_LIFFE, GetUnderlyingAsset_LIFFE
       Derivatives Contracts with Symbol: 
          PBG   (Eris GBP LIBOR Interest Rate Future 6-month Calendar, 2 Years)
	        PCG   (Eris GBP LIBOR Interest Rate Future 6-month Calendar, 3 Years)
	        PEG   (Eris GBP LIBOR Interest Rate Future 6-month Calendar, 5 Years)
	        PGH   (Eris GBP LIBOR Interest Rate Future 6-month Calendar, 7 Years)
	        PJH   (Eris GBP LIBOR Interest Rate Future 6-month Calendar, 10 Years)
	        PNI   (Eris GBP LIBOR Interest Rate Future 6-month Calendar, 30 Years)
	        RGE   (Eris EURIBOR Interest Rate Future 6-month Calendar, 7 Years)
	        RJG   (Eris EURIBOR Interest Rate Future 6-month Calendar, 10 Years)
	        RNH   (Eris EURIBOR Interest Rate Future 6-month Calendar, 30 Years)
=========================================================================================================          
Version : v5.1.6113
	Date    : 20160926
	Author  : FL&PLA
	Comment : [22492] ICE-EU - Repository error : Row is rejected by controls. [EFS]
	  - Management in templates: GetSettltMethod_LIFFE, GetAssetCategory_LIFFE, GetUnderlyingAsset_LIFFE
       Derivatives Contracts with Symbol:
          PGK (Eris GBP LIBOR Interest Rate Future 6-month Calendar, 7 Years)
          REE (Eris EURIBOR Interest Rate Future 6-month Calendar, 5 Years)
          RGF (Eris EURIBOR Interest Rate Future 6-month Calendar, 7 Years)
=========================================================================================================
Version : v5.1.6016
	Date    : 20160621
	Author  : FL
	Comment : [22266] ICE-EU - Repository error : Row is rejected by controls. [EFS]
 	  - Management in templates: GetSettltMethod_LIFFE, GetAssetCategory_LIFFE, GetUnderlyingAsset_LIFFE
	     Derivatives Contracts with Symbol:
          PCI (Eris GBP LIBOR Interest Rate Future 6-month Calendar, 3 Years)
          PEJ (Eris GBP LIBOR Interest Rate Future 6-month Calendar, 5 Years)
          PGJ (Eris GBP LIBOR Interest Rate Future 6-month Calendar, 7 Years)
          PJK (Eris GBP LIBOR Interest Rate Future 6-month Calendar, 10 Years)
          PNL (Eris GBP LIBOR Interest Rate Future 6-month Calendar, 30 Years)
          RJH (Eris EURIBOR Interest Rate Future 6-month Calendar, 10 Years)
          RNJ (Eris EURIBOR Interest Rate Future 6-month Calendar, 30 Years)
=========================================================================================================
Version : v5.1.5925
	Date    : 20160322
	Author  : FL
	Comment : [22002] ICE-EU - Repository error : Row is rejected by controls
	  - Management in templates: GetSettltMethod_LIFFE, GetAssetCategory_LIFFE, GetUnderlyingAsset_LIFFE
	     Derivatives Contracts with Symbol:
          PEL (Eris GBP LIBOR Interest Rate Future 6-month Calendar, 5 Years)          
=========================================================================================================
Version : v5.0.5834
	Date    : 20151222
	Author  : FL
	Comment : [21678] ICE-EU - Repository error : Row is rejected by controls
	  - Management in templates: GetSettltMethod_LIFFE, GetAssetCategory_LIFFE, GetUnderlyingAsset_LIFFE
	     Derivatives Contracts with Symbol :
          RCE (Eris EURIBOR® Interest Rate Future 6-month Calendar, 3 Years)
          REF (Eris EURIBOR® Interest Rate Future 6-month Calendar, 5 Years)
=========================================================================================================
Version : v5.0.5833
	Date    : 20151221
	Author  : FL
	Comment : [21678] ICE-EU - Repository error : Row is rejected by controls
	  - Management in templates: GetSettltMethod_LIFFE, GetAssetCategory_LIFFE, GetUnderlyingAsset_LIFFE
	     Derivatives Contracts with Symbol:
          PNM (Eris GBP LIBOR Interest Rate Future 6-month Calendar, 30 Years)
          PGL (Eris GBP LIBOR Interest Rate Future 6-month Calendar, 7 Years)          
=========================================================================================================
Version : v5.0.5807
	Date    : 20151125
	Author  : FL
	Comment : [21570] ICE-EU - Repository error : Row is rejected by controls
	  - Management in templates: GetSettltMethod_LIFFE, GetAssetCategory_LIFFE, GetUnderlyingAsset_LIFFE
	     Derivatives Contracts with Symbol:
          VY 	(Eris GBP LIBOR Interest Rate Future 3-month IMM, 10 Years)
          VJ 	(Eris GBP LIBOR Interest Rate Future 6-month IMM, 10 Years)
          PY 	(Eris GBP LIBOR Interest Rate Future 3-month Calendar, 10 Years)
          PJ 	(Eris GBP LIBOR Interest Rate Future 6-month Calendar, 10 Years)
          PNN (Eris GBP LIBOR Interest Rate Future 6-month Calendar, 30 Years)
          QY	(Eris EURIBOR® Interest Rate Future 3-month IMM, 10 Years)
          QJ 	(Eris EURIBOR® Interest Rate Future 6-month IMM, 10 Years)
          RZ 	(Eris EURIBOR® Interest Rate Future 3-month Calendar, 10 Years)
          RJ	(Eris EURIBOR® Interest Rate Future 6-month Calendar, 10 Years)
          RNK	(Eris EURIBOR® Interest Rate Future 6-month Calendar, 30 Years)
=========================================================================================================
  Version : v4.5.5743
	Date    : 20150922
	Author  : FL
	Comment : [21379] ICE-EU - Repository error : Row is rejected by controls
	  - Management in templates: GetSettltMethod_LIFFE, GetAssetCategory_LIFFE, GetUnderlyingAsset_LIFFE
	     Derivatives Contracts with Symbol:
         	PBJ	(Eris 2 Year 1.25% Standard GBP Interest Rate Future)
		      PCK	(Eris 3 Year 1.50% Standard GBP Interest Rate Future)
		      PEM	(Eris 5 Year 2.00% Standard GBP Interest Rate Future)
		      RBF	(Eris 2 Year 0.25% Standard EUR Interest Rate Future)
		      RGH	(Eris 7 Year 0.75% Standard EUR Interest Rate Future)
		      RJJ	(Eris 10 Year 1.25% Standard EUR Interest Rate Future)
=========================================================================================================
  Version : v4.5.5626
  Date    : 20150703
  Author  : FL
  Comment : [21164] ICE-EU - Repository error : Row is rejected by controls
    - Management in templates: GetSettltMethod_LIFFE, GetAssetCategory_LIFFE, GetUnderlyingAsset_LIFFE
       Derivatives Contracts with Symbol:
          PBI	(Eris 2YR GBP Interest Rate Future)
		      PCJ	(Eris 3YR GBP Interest Rate Future)
		      PEK	(Eris 5YR GBP Interest Rate Future)
		      PGM	(Eris 7YR GBP Interest Rate Future)
		      PJM	(Eris 10YR GBP Interest Rate Future)
		      RBE	(Eris 2YR EUR Interest Rate Future)
		      RCF	(Eris 3YR EUR Interest Rate Future)
		      REG	(Eris 5YR EUR Interest Rate Future)
		      RGG	(Eris 7YR EUR Interest Rate Future)
		      RJI	(Eris 10YR EUR Interest Rate Future)
=========================================================================================================
  Version : v4.5.5618
  Date    : 20150520
  Author  : FL
  Comment : [21037] ICE-EU - Repository error : Row is rejected by controls
    - Management in templates: GetSettltMethod_LIFFE, GetAssetCategory_LIFFE, GetUnderlyingAsset_LIFFE
       Derivatives Contracts with Symbol:
          ED (Eurodollar Futures)
	        RPA (Agency DTCC GCF Repo Index) 
	        RPM (Mortgage Backed Securities DTCC GCF Repo Index)
	        RPT (U.S. Treasury DTCC GCF Repo Index)
=========================================================================================================
  Version : v4.5.5522
  Date    : 20150217
  Author  : FL
  Comment : [20802] ICE-EU - Repository error : Row is rejected by controls
    - Management in templates: GetSettltMethod_LIFFE, GetAssetCategory_LIFFE, GetUnderlyingAsset_LIFFE,
       ovrSQLGetDerivativeContractDefaultValue Derivatives Contracts with Symbol :
          K5 (Option on Three Month Euro)
          M5 (Option on Three Month Sterling)
=========================================================================================================
 Version : v4.2.5388
  Date    : 20141002
  Author  : PL
  Comment : [19669] New MIC 
    Before
        XLIF - EURONEXT.LIFFE         (ISO: XLIF, Symbol du marché: L) for INDEX 
        EQY  - EURONEXT.LIFFE EQUITY  (ISO: XLIF, Symbol du marché: O) for EQUITY 
    Now
        IFLL - ICE FUTURES EUROPE - FINANCIAL PRODUCTS DIVISION (ISO: IFLL, Symbol du marché: L) for FINANCIAL PRODUCTS 
        IFLO - ICE FUTURES EUROPE - EQUITY PRODUCTS DIVISION    (ISO: IFLO, Symbol du marché: O) for EQUITY PRODUCTS 
=========================================================================================================
  Version : v4.0.0.0
  Date    : 20140619
  Author  : FL
  Comment : [20113] le sous-jacents (colonne(IDDC_UNL) sur les Options sur Futures n'est pas mise à jour ainsi 
            que le contract size(colonne(FACTOR)pour tous les DC.
=========================================================================================================
  Version : v4.1.0.0
  Date    : 20140512
  Author  : PM
  Comment : [19970][19259] Management of currency from ICEEUPRODUCT
=========================================================================================================
  Version : v4.0.0.0
  Date    : 20140425
  Author  : FL
  Comment : [19648] 
    - Management in templates GetSettltMethod_LIFFE, GetAssetCategory_LIFFE, GetUnderlyingAsset_LIFFE,
      Derivatives Contracts with Symbol:
          EU3 (30Yr Euro Swapnote)
          GB3 (30Yr Sterling Swapnote)
          US3 (30Yr US Dollar Swapnote)
          C05 (MEDIUM SWISS CONFEDERATION BOND FUTURE)
          C10 (LONG SWISS CONFEDERATION BOND FUTURE)
          G02 (SHORT BUND FUTURE)
          G05 (MEDIUM BUND FUTURE)
          G10 (LONG BUND FUTURE)
          G30 (ULTRA LONG BUND FUTURE
          I02 (SHORT BTP FUTURE)
          I05 (MEDIUM BTP FUTURE)
          I10 (LONG BTP FUTURE)
          UEW (FTSE 100 Equally Wei)
=========================================================================================================
  Version : v4.0.0.0
  Date    : 20140326
  Author  : RD
  Comment : 
    - Create and use template "GetExchangeSymbol_LIFFE"
    - Add a new optional parameter "pParamExchangeSymbol" to template "SQLTableDERIVATIVECONTRACT"
       It contains a Spheres I/O "Param" element
=========================================================================================================
  Version : v4.0.0.0
  Date    : 20140325
  Author  : FL
  Comment : [19648] 
    - Management in templates GetSettltMethod_LIFFE, GetAssetCategory_LIFFE, GetUnderlyingAsset_LIFFE,
      Derivatives Contracts with Symbol:
          U (Ultra Long Gilt Futures)
          UKM( Russell UK MID 150)
          CHW (2Yr Swiss Swapnote)
          CHO (5Yr Swiss Swapnote)
          CHP (10Yr Swiss Swapnote)
          GBW (2Yr Sterling Swapnote)
          GBO (5Yr Sterling Swapnote)
          GBP (10Yr Sterling Swapnote)
                     
    - Template creation GetUnderlyingAsset_LIFFE
       (return UnderlyingAsset from an LIFFE Derivative Contract)
       
    - Template creation GetExchangeSymbol_LIFFE
      (return ExchangeSymbol from an LIFFE Derivative Contract)
=========================================================================================================
  Version : v3.7.0.5088
  Date    : 20131129
  Author  : FI
  Comment : [19284] Alimentation de DERIVATIVECONTRACT.FINALSETTLTSIDE
=========================================================================================================
BD 20130314 Create file
=========================================================================================================
-->

<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
  <xsl:output method="xml" omit-xml-declaration="no" encoding="UTF-8" indent="yes" media-type="text/xml; charset=ISO-8859-1"/>

  <!-- ================================================== -->
  <!--        include(s)                                  -->
  <!-- ================================================== -->
  <xsl:include href="MarketData_Common.xsl"/>
  <xsl:include href="MarketData_ICE-EUCommon_ICE-EUProduct_Import_Map.xsl"/>
  
  <!-- ================================================== -->
  <!--        Variables                                   -->
  <!-- ================================================== -->  
  <xsl:variable name="smallcase" select="'abcdefghijklmnopqrstuvwxyz'" />
  <xsl:variable name="uppercase" select="'ABCDEFGHIJKLMNOPQRSTUVWXYZ'" />    

  <!-- ================================================== -->
  <!--        GetSettltMethod_LIFFE                       -->
  <!-- ================================================== -->
  <xsl:template name="GetSettltMethod_LIFFE">
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pContractDescription"/>
    
    <xsl:variable name="vContractDescription" select="translate(normalize-space($pContractDescription),$smallcase,$uppercase)" />

    <!-- Assignment of SettltMethod for LIFFE based on the following matrix:        
           pContractSymbol        pCategory       pContractDescription    SettltMethod
           ===========================================================================
           EON	                  F	                                      C
           EO3	                  F	                                      C
           I	                    F	                                      C
           J	                    F	                                      C
           L	                    F	                                      C
           O	                    F	                                      C
           P	                    F	                                      C
           S	                    F	                                      C
           TWS	                  F	                                      C
           CHW	                  F	                                      C
           CHO	                  F	                                      C
           CHP	                  F	                                      C
           GBW	                  F	                                      C
           GBO	                  F	                                      C
           GBP	                  F	                                      C
           USW	                  F	                                      C
           USO	                  F	                                      C
           USP	                  F	                                      C
           UKM	                  F	                                      C
           UEW                    F                                       C
           EU3	                  F	                                      C
           GB3	                  F	                                      C
           US3	                  F	                                      C
           ED	                    F	                                      C
           P..                    F               ...ERIS...              C
		       R..                    F               ...ERIS...              C
           V..                    F               ...ERIS...              C
           Q..                    F               ...ERIS...              C
           SOA                    F                                       C
           SO3                    F                                       C
           EUN		                F                                       C
           EUO		                F                                       C
           EUP		                F                                       C
           GBS		                F                                       C
           GBY		                F                                       C
           2F.->2O. (1->10 year)  F               ...ERIS...              C
           2S.                    F               ...ERIS...              C
           3F.->2O. (1->10 year)  F               ...ERIS...              C
           3S.                    F               ...ERIS...              C
           6F.->6O. (1->10 year)  F               ...ERIS...              C
           6S..                   F               ...ERIS...              C
           G	                    F	                                      P
           U	                    F	                                      P
           H	                    F	                                      P
           JGB	                  F	                                      P
           R	                    F	                                      P
           C05	                  F	                                      P
           C10	                  F	                                      P
           G02	                  F	                                      P
           G05	                  F	                                      P
           G10	                  F	                                      P
           G30	                  F	                                      P 
           I02	                  F	                                      P
           I05	                  F	                                      P
           I10	                  F	                                      P
           SA3                    F	                                      P
           SF1                    F	                                      P
           SF3                    F	                                      P
           I	                    O	                                      P
           K	                    O	                                      P
           K2	                    O	                                      P
           K3	                    O	                                      P
           K4	                    O	                                      P
           K5	                    O	                                      P
           M	                    O	                                      P
           M2	                    O	                                      P
           M3	                    O	                                      P
           M4	                    O	                                      P
           M5	                    O	                                      P
           L	                    O	                                      P
           O	                    O	                                      P
           P	                    O	                                      P
           R	                    O	                                      P
           S	                    O	                                      P
           TWS	                  O	                                      P
           SO3                    O                                       P
           SY1                    O                                       P
           SY2                    O                                       P
           SY3                    O                                       P
           SY4                    O                                       P
           SY5                    O                                       P -->
           
    <xsl:choose>
      <xsl:when test="$pCategory='F' and ($pContractSymbol='EO3' or
                                          $pContractSymbol='EON' or
                                          $pContractSymbol='I'   or
                                          $pContractSymbol='J'   or                      
                                          $pContractSymbol='L'   or                      
                                          $pContractSymbol='O'   or
                                          $pContractSymbol='P'   or
                                          $pContractSymbol='S'   or
                                          $pContractSymbol='TWS' or
                                          $pContractSymbol='CHW' or
                                          $pContractSymbol='CHO' or
                                          $pContractSymbol='CHP' or
                                          $pContractSymbol='GBW' or
                                          $pContractSymbol='GBO' or
                                          $pContractSymbol='GBP' or
                                          $pContractSymbol='USW' or
                                          $pContractSymbol='USO' or
                                          $pContractSymbol='USP' or
                                          $pContractSymbol='UKM' or
                                          $pContractSymbol='UEW' or
                                          $pContractSymbol='EU3' or
                                          $pContractSymbol='GB3' or
                                          $pContractSymbol='US3' or
                                          $pContractSymbol='ED' or
                                          (starts-with($pContractSymbol,'R') and contains($vContractDescription,'ERIS')) or
                                          (starts-with($pContractSymbol,'Q') and contains($vContractDescription,'ERIS')) or
                                          (starts-with($pContractSymbol,'P') and contains($vContractDescription,'ERIS')) or
                                          (starts-with($pContractSymbol,'V') and contains($vContractDescription,'ERIS')) or
                                          $pContractSymbol='SOA' or
                                          $pContractSymbol='SO3' or 
                                          $pContractSymbol='EUN' or
                                          $pContractSymbol='EUO' or
                                          $pContractSymbol='EUP' or
                                          $pContractSymbol='GBS' or
                                          $pContractSymbol='GBY' or 
                                          (contains(',2F,2G,2H,2I,2J,2K,2L,2M,2N,2O,2S,',concat(',',substring($pContractSymbol,1,2),',')) and contains($vContractDescription,'ERIS')) or
                                          (contains(',3F,3G,3H,3I,3J,3K,3L,3M,3N,3O,3S,',concat(',',substring($pContractSymbol,1,2),',')) and contains($vContractDescription,'ERIS')) or
                                          (contains(',6F,6G,6H,6I,6J,6K,6L,6M,6N,6O,6S,',concat(',',substring($pContractSymbol,1,2),',')) and contains($vContractDescription,'ERIS')) 
                                         )">C</xsl:when>

      <xsl:when test="$pCategory='F' and ($pContractSymbol='G'   or
                                          $pContractSymbol='U'   or
                                          $pContractSymbol='H'   or
                                          $pContractSymbol='JGB' or
                                          $pContractSymbol='R'   or
                                          $pContractSymbol='C05' or
                                          $pContractSymbol='C10' or
                                          $pContractSymbol='SA3' or
                                          $pContractSymbol='SF1' or
                                          $pContractSymbol='SF3' or
                                          $pContractSymbol='G02' or
                                          $pContractSymbol='G05' or
                                          $pContractSymbol='G10' or
                                          $pContractSymbol='G30' or
                                          $pContractSymbol='I02' or
                                          $pContractSymbol='I05' or
                                          $pContractSymbol='I10'
                                         )">P</xsl:when>
      
      <xsl:when test="$pCategory='O' and ($pContractSymbol='I'   or
                                          $pContractSymbol='K'   or
                                          $pContractSymbol='K2'  or
                                          $pContractSymbol='K3'  or
                                          $pContractSymbol='K4'  or
                                          $pContractSymbol='K5'  or
                                          $pContractSymbol='M'   or
                                          $pContractSymbol='M2'  or
                                          $pContractSymbol='M3'  or
                                          $pContractSymbol='M4'  or
                                          $pContractSymbol='M5'  or
                                          $pContractSymbol='L'   or
                                          $pContractSymbol='O'   or
                                          $pContractSymbol='P'   or
                                          $pContractSymbol='R'   or
                                          $pContractSymbol='S'   or
                                          $pContractSymbol='TWS' or
                                          $pContractSymbol='SO3' or
                                          $pContractSymbol='SY1' or
                                          $pContractSymbol='SY2' or
                                          $pContractSymbol='SY3' or
                                          $pContractSymbol='SY4' or
                                          $pContractSymbol='SY5'
                                         )">P</xsl:when>

    </xsl:choose>
  </xsl:template>

  <!-- ================================================== -->
  <!--        GetAssetCategory_LIFFE                      -->
  <!-- ================================================== -->
  <!-- FI/PM 20141002 [19669] add S02,S05,S10  -->
  <xsl:template name="GetAssetCategory_LIFFE">
    <xsl:param name="pExchangeSymbol"/>
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pContractDescription"/>
    
    <xsl:variable name="vContractDescription" select="translate(normalize-space($pContractDescription),$smallcase,$uppercase)" />

    <!-- Assignment of AssetCategory for LIFFE based on the following matrix:        
           pContractSymbol        pCategory       vContractDescription    SettltMethod
           ===========================================================================
           EON	                  F	                                      RateIndex
           EO3	                  F	                                      RateIndex
           I	                    F	                                      RateIndex
           J		                  F	                                      RateIndex
           L		                  F	                                      RateIndex
           O		                  F	                                      RateIndex
           P		                  F	                                      RateIndex
           S		                  F	                                      RateIndex
           TWS	                  F	                                      RateIndex
           CHW	                  F	                                      RateIndex
           CHO	                  F	                                      RateIndex
           CHP	                  F	                                      RateIndex
           GBW	                  F	                                      RateIndex
           GBO	                  F	                                      RateIndex
           GBP	                  F	                                      RateIndex
           USW	                  F	                                      RateIndex
           USO	                  F	                                      RateIndex
           USP	                  F	                                      RateIndex
           EU3	                  F	                                      RateIndex
           GB3	                  F	                                      RateIndex
           US3	                  F	                                      RateIndex
           ED	                    F	                                      RateIndex
           P..                    F               ...ERIS...              RateIndex
		       R..                    F               ...ERIS...              RateIndex
           V..                    F               ...ERIS...              RateIndex
           Q..                    F               ...ERIS...              RateIndex
           SOA                    F                                       RateIndex
           SO3                    F                                       RateIndex
           EUN		                F                                       RateIndex
           EUO		                F                                       RateIndex
           EUP		                F                                       RateIndex
           GBS		                F                                       RateIndex
           GBY		                F                                       RateIndex
           2F.->2O. (1->10 year)  F               ...ERIS...              RateIndex
           2S.                    F               ...ERIS...              RateIndex
           3F.->2O. (1->10 year)  F               ...ERIS...              RateIndex
           3S.                    F               ...ERIS...              RateIndex
           6F.->6O. (1->10 year)  F               ...ERIS...              RateIndex
           6S..                   F               ...ERIS...              RateIndex
           G	                    F	                                      Bond
           U	                    F	                                      Bond
           H	                    F	                                      Bond
           R	                    F	                                      Bond
           JGB	                  F	                                      Bond
           C05	                  F	                                      Bond
           C10	                  F	                                      Bond
           G02	                  F	                                      Bond
           G05	                  F	                                      Bond
           G10	                  F	                                      Bond
           G30	                  F	                                      Bond
           I02	                  F	                                      Bond
           I05	                  F	                                      Bond
           I10	                  F	                                      Bond
           K	                    O	                                      Future
           K2	                    O	                                      Future
           K3	                    O	                                      Future
           K4	                    O	                                      Future
           K5	                    O	                                      Future
           M	                    O	                                      Future
           M2	                    O	                                      Future
           M3	                    O	                                      Future
           M4	                    O	                                      Future
           M5	                    O	                                      Future
           I	                    O	                                      Future
           L	                    O	                                      Future
           O	                    O	                                      Future
           P	                    O	                                      Future
           R	                    O	                                      Future
           S	                    O	                                      Future
           TWS	                  O	                                      Future
           SO3                    O                                       Future
           SY1                    O                                       Future
           SY2                    O                                       Future
           SY3                    O                                       Future
           SY4                    O                                       Future
           SY5                    O                                       Future
           UKM	                  F	                                      Index
           UEW	                  F	                                      Index
           RPA	                  F	                                      Index
           RPM	                  F	                                      Index
           RPT	                  F	                                      Index
           
           Ps. Special case, I only found this rule for the moment to be deepened after V4.
           If pContractDescription contains ('MSCI' or 'TOPIX' or 'Index' or 'INDEX') and pCategory = 'F' then AssetCategory = 'Index' -->
    
    <xsl:choose>      
      <xsl:when test="$pCategory='F' and ($pContractSymbol='EO3' or
                                          $pContractSymbol='EON' or
                                          $pContractSymbol='I'   or
                                          $pContractSymbol='J'   or                      
                                          $pContractSymbol='L'   or                     
                                          $pContractSymbol='O'   or
                                          $pContractSymbol='P'   or
                                          $pContractSymbol='S'   or
                                          $pContractSymbol='TWS' or
                                          $pContractSymbol='CHW' or
                                          $pContractSymbol='CHO' or
                                          $pContractSymbol='CHP' or
                                          $pContractSymbol='GBW' or
                                          $pContractSymbol='GBO' or
                                          $pContractSymbol='GBP' or
                                          $pContractSymbol='USW' or
                                          $pContractSymbol='USO' or
                                          $pContractSymbol='USP' or
                                          $pContractSymbol='EU3' or
                                          $pContractSymbol='GB3' or
                                          $pContractSymbol='US3' or
                                          $pContractSymbol='ED'  or
                                          (starts-with($pContractSymbol,'R') and contains($vContractDescription,'ERIS')) or
                                          (starts-with($pContractSymbol,'Q') and contains($vContractDescription,'ERIS')) or
                                          (starts-with($pContractSymbol,'P') and contains($vContractDescription,'ERIS')) or
                                          (starts-with($pContractSymbol,'V') and contains($vContractDescription,'ERIS')) or
                                          $pContractSymbol='SA3' or
                                          $pContractSymbol='SOA' or
                                          $pContractSymbol='SF1' or
                                          $pContractSymbol='SF3' or
                                          $pContractSymbol='SO3' or 
                                          $pContractSymbol='EUN' or
                                          $pContractSymbol='EUO' or
                                          $pContractSymbol='EUP' or
                                          $pContractSymbol='GBS' or
                                          $pContractSymbol='GBY' or
                                          (contains(',2F,2G,2H,2I,2J,2K,2L,2M,2N,2O,2S,',concat(',',substring($pContractSymbol,1,2),',')) and contains($vContractDescription,'ERIS')) or
                                          (contains(',3F,3G,3H,3I,3J,3K,3L,3M,3N,3O,3S,',concat(',',substring($pContractSymbol,1,2),',')) and contains($vContractDescription,'ERIS')) or
                                          (contains(',6F,6G,6H,6I,6J,6K,6L,6M,6N,6O,6S,',concat(',',substring($pContractSymbol,1,2),',')) and contains($vContractDescription,'ERIS'))
                                         )">RateIndex</xsl:when>

      <xsl:when test="$pCategory='F' and ($pContractSymbol='G'   or
                                          $pContractSymbol='U'   or
                                          $pContractSymbol='H'   or
                                          $pContractSymbol='JGB' or
                                          $pContractSymbol='R'   or
                                          $pContractSymbol='C05' or
                                          $pContractSymbol='C10' or
                                          $pContractSymbol='G02' or
                                          $pContractSymbol='G05' or
                                          $pContractSymbol='G10' or
                                          $pContractSymbol='G30' or
                                          $pContractSymbol='I02' or
                                          $pContractSymbol='I05' or
                                          $pContractSymbol='S02' or
                                          $pContractSymbol='S05' or
                                          $pContractSymbol='S10' or
                                          $pContractSymbol='I10'
                                         )">Bond</xsl:when>

      <xsl:when test="$pCategory='O' and ($pContractSymbol='K'   or
                                          $pContractSymbol='K2'  or
                                          $pContractSymbol='K3'  or
                                          $pContractSymbol='K4'  or
                                          $pContractSymbol='K5'  or
                                          $pContractSymbol='M'   or                                          
                                          $pContractSymbol='M2'  or
                                          $pContractSymbol='M3'  or
                                          $pContractSymbol='M4'  or
                                          $pContractSymbol='M5'  or
                                          $pContractSymbol='I'   or
                                          $pContractSymbol='L'   or
                                          $pContractSymbol='O'   or
                                          $pContractSymbol='P'   or
                                          $pContractSymbol='R'   or
                                          $pContractSymbol='S'   or
                                          $pContractSymbol='TWS' or
                                          $pContractSymbol='SO3' or
                                          $pContractSymbol='SY1' or
                                          $pContractSymbol='SY2' or
                                          $pContractSymbol='SY3' or
                                          $pContractSymbol='SY4' or
                                          $pContractSymbol='SY5'
                                         )">Future</xsl:when>

      <xsl:when test="$pCategory='F' and ($pContractSymbol='UKM' or
                                          $pContractSymbol='UEW' or
                                          $pContractSymbol='RPA' or
                                          $pContractSymbol='RPM' or
                                          $pContractSymbol='RPT' or
                                          contains($pContractDescription,'MSCI')  or
                                          contains($pContractDescription,'TOPIX') or
                                          contains($pContractDescription,'INDEX') or
                                          contains($pContractDescription,'Index')
                                         )">Index</xsl:when>
    
      <xsl:when test="$pExchangeSymbol='O'">EquityAsset</xsl:when>
    </xsl:choose>
  </xsl:template>

  <!-- FL 20140318 [19648] Creation Template : GetUnderlyingAsset_LIFFE  -->     
  <!-- ================================================================= -->
  <!--        GetUnderlyingAsset_LIFFE                                   -->
  <!-- ================================================================= -->
  <xsl:template name="GetUnderlyingAsset_LIFFE">
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pContractDescription"/>
    
    <xsl:variable name="vContractDescription" select="translate(normalize-space($pContractDescription),$smallcase,$uppercase)" />

    <!--  Assignment of UnderlyingAsset for LIFFE based on the following matrix:   
           pContractSymbol        pCategory      vContractDescription UnderlyingAsset
           =================================================================================
           EON	                  F	                                  FD (Interest rate/notional debt securities)
           EO3	                  F	                                  FD (Interest rate/notional debt securities)
           I	                    F	                                  FD (Interest rate/notional debt securities)
           J	                    F	                                  FD (Interest rate/notional debt securities)
           L	                    F	                                  FD (Interest rate/notional debt securities)
           O	                    F	                                  FD (Interest rate/notional debt securities)
           P	                    F	                                  FD (Interest rate/notional debt securities)
           S	                    F	                                  FD (Interest rate/notional debt securities)
           TWS	                  F	                                  FD (Interest rate/notional debt securities)
           CHW	                  F	                                  FD (Interest rate/notional debt securities)
           CHO	                  F	                                  FD (Interest rate/notional debt securities)
           CHP	                  F	                                  FD (Interest rate/notional debt securities)
           GBW	                  F	                                  FD (Interest rate/notional debt securities)
           GBO	                  F	                                  FD (Interest rate/notional debt securities)
           GBP	                  F	                                  FD (Interest rate/notional debt securities)
           USW	                  F	                                  FD (Interest rate/notional debt securities)
           USO	                  F	                                  FD (Interest rate/notional debt securities)
           USP	                  F	                                  FD (Interest rate/notional debt securities)
           G	                    F	                                  FD (Interest rate/notional debt securities)
           U	                    F	                                  FD (Interest rate/notional debt securities)
           H	                    F	                                  FD (Interest rate/notional debt securities)
           JGB	                  F	                                  FD (Interest rate/notional debt securities)
           R	                    F	                                  FD (Interest rate/notional debt securities)
           EU3	                  F	                                  FD (Interest rate/notional debt securities)
           GB3	                  F	                                  FD (Interest rate/notional debt securities)
           US3	                  F	                                  FD (Interest rate/notional debt securities)
           C05	                  F	                                  FD (Interest rate/notional debt securities)
           C10	                  F	                                  FD (Interest rate/notional debt securities)
           G02	                  F	                                  FD (Interest rate/notional debt securities)
           G05	                  F	                                  FD (Interest rate/notional debt securities)
           G10	                  F	                                  FD (Interest rate/notional debt securities)
           G30	                  F	                                  FD (Interest rate/notional debt securities)
           I02	                  F	                                  FD (Interest rate/notional debt securities)
           I05	                  F	                                  FD (Interest rate/notional debt securities)
           I10	                  F	                                  FD (Interest rate/notional debt securities)
           ED	                    F	                                  FD (Interest rate/notional debt securities)
           P..                    F              ...ERIS...           FD (Interest rate/notional debt securities)
           R..                    F              ...ERIS...           FD (Interest rate/notional debt securities)
           V..                    F              ...ERIS...           FD (Interest rate/notional debt securities)
           Q..                    F              ...ERIS...           FD (Interest rate/notional debt securities)
           SA3                    F                                   FD (Interest rate/notional debt securities)
           SOA                    F                                   FD (Interest rate/notional debt securities)
           SO3                    F                                   FD (Interest rate/notional debt securities)
           SF1                    F                                   FD (Interest rate/notional debt securities)   
           SF3                    F                                   FD (Interest rate/notional debt securities) 
           EUN		                F                                   FD (Interest rate/notional debt securities)
           EUO		                F                                   FD (Interest rate/notional debt securities)
           EUP		                F                                   FD (Interest rate/notional debt securities)
           GBS		                F                                   FD (Interest rate/notional debt securities)
           GBY		                F                                   FD (Interest rate/notional debt securities)
           2F.->2O. (1->10 year)  F              ...ERIS...           FD (Interest rate/notional debt securities)
           2S.                    F              ...ERIS...           FD (Interest rate/notional debt securities)
           3F.->2O. (1->10 year)  F              ...ERIS...           FD (Interest rate/notional debt securities)
           3S.                    F              ...ERIS...           FD (Interest rate/notional debt securities)
           6F.->6O. (1->10 year)  F              ...ERIS...           FD (Interest rate/notional debt securities)
           6S..                   F              ...ERIS...           FD (Interest rate/notional debt securities)
           UKM	                  F	                                  FI (Indices)
           UEW	                  F	                                  FI (Indices)
           RPA	                  F	                                  FI (Indices)
           RPM	                  F	                                  FI (Indices)
           RPT	                  F	                                  FI (Indices)
           
           Ps. Special case, I only found this rule for the moment to be deepened after V4.
           If pContractDescription cotient ('MSCI' or 'TOPIX' or 'Index' or 'INDEX') and pCategory = 'F' then UnderlyingAsset = FI ('Indices') -->
                   
    <xsl:choose>
      <xsl:when test="$pCategory='F' and ($pContractSymbol='EON' or
                                          $pContractSymbol='EO3' or
                                          $pContractSymbol='I'   or
                                          $pContractSymbol='J'   or
                                          $pContractSymbol='L'   or
                                          $pContractSymbol='O'   or
                                          $pContractSymbol='P'   or
                                          $pContractSymbol='S'   or
                                          $pContractSymbol='TWS' or
                                          $pContractSymbol='CHW' or
                                          $pContractSymbol='CHO' or
                                          $pContractSymbol='CHP' or
                                          $pContractSymbol='GBW' or
                                          $pContractSymbol='GBO' or
                                          $pContractSymbol='GBP' or
                                          $pContractSymbol='USW' or
                                          $pContractSymbol='USO' or
                                          $pContractSymbol='USP' or
                                          $pContractSymbol='G'   or
                                          $pContractSymbol='U'   or
                                          $pContractSymbol='H'   or
                                          $pContractSymbol='JGB' or
                                          $pContractSymbol='R'   or
                                          $pContractSymbol='EU3' or
                                          $pContractSymbol='GB3' or
                                          $pContractSymbol='US3' or
                                          $pContractSymbol='C05' or
                                          $pContractSymbol='C10' or
                                          $pContractSymbol='G02' or
                                          $pContractSymbol='G05' or
                                          $pContractSymbol='G10' or
                                          $pContractSymbol='G30' or
                                          $pContractSymbol='I02' or
                                          $pContractSymbol='I05' or
                                          $pContractSymbol='I10' or
                                          $pContractSymbol='ED'  or
                                          (starts-with($pContractSymbol,'R') and contains($vContractDescription,'ERIS')) or
                                          (starts-with($pContractSymbol,'Q') and contains($vContractDescription,'ERIS')) or
                                          (starts-with($pContractSymbol,'P') and contains($vContractDescription,'ERIS')) or
                                          (starts-with($pContractSymbol,'V') and contains($vContractDescription,'ERIS')) or
                                          $pContractSymbol='SA3' or 
                                          $pContractSymbol='SF1' or 
                                          $pContractSymbol='SF3' or 
                                          $pContractSymbol='SOA' or 
                                          $pContractSymbol='SA3' or
                                          $pContractSymbol='SO3' or 
                                          $pContractSymbol='EUN' or
                                          $pContractSymbol='EUO' or
                                          $pContractSymbol='EUP' or
                                          $pContractSymbol='GBS' or
                                          $pContractSymbol='GBY' or
                                          (contains(',2F,2G,2H,2I,2J,2K,2L,2M,2N,2O,2S,',concat(',',substring($pContractSymbol,1,2),',')) and contains($vContractDescription,'ERIS')) or
                                          (contains(',3F,3G,3H,3I,3J,3K,3L,3M,3N,3O,3S,',concat(',',substring($pContractSymbol,1,2),',')) and contains($vContractDescription,'ERIS')) or
                                          (contains(',6F,6G,6H,6I,6J,6K,6L,6M,6N,6O,6S,',concat(',',substring($pContractSymbol,1,2),',')) and contains($vContractDescription,'ERIS'))
                                         )">FD</xsl:when>

      <xsl:when test="$pCategory='F' and ($pContractSymbol='UKM' or
                                          $pContractSymbol='UEW' or
                                          $pContractSymbol='RPA' or
                                          $pContractSymbol='RPM' or
                                          $pContractSymbol='RPT' or
                                          contains($pContractDescription,'MSCI')  or
                                          contains($pContractDescription,'TOPIX') or
                                          contains($pContractDescription,'INDEX') or
                                          contains($pContractDescription,'Index')
                                         )">FI</xsl:when>

      <xsl:otherwise>FS</xsl:otherwise>
      
    </xsl:choose>
  </xsl:template>


  <!-- FL/RD 20140326 [19648] Creation Template : GetExchangeSymbol_LIFFE -->
  <!-- FL/RD 20140425 [19648] Parameter management pCategory -->
  <!-- ==============================================================================  -->
  <!--        GetExchangeSymbol_LIFFE                                                  -->
  <!-- ==============================================================================  -->
  <!--  Règle:                                                                         -->
  <!--    - Tous les DC dont ASSETCATEGORY = 'EquityAsset' vont dans le segment        -->
  <!--       de marché 'LIFFE EQUITY'(ExchangeSymbol = 'O')                            -->
  <!--    - Sinon ils vont dans le marché ‘LIFFE’ (ExchangeSymbol= 'L')                -->
  <!-- ==============================================================================  -->
  

  <!-- FI/PM 20141002 [19669] Modifying the Template adding the parameter pExchangeSymbol
  To correct a problem on GBP contracts (there is 1 STD contract and 1 a FLEX contract)
  The previous request gave priority to ORIGINEDATA = 'BCLEAR' and therefore the exchange symbol retrieved was 'O' instead of 'L' on the Standard contract
  Similarly Sphere® applied ASSETCATEGORY = 'EquityAsset' for the standard contract whereas it is a Bond
  The modification consists in taking into consideration the pExchangeSymbol parameter whose value is
  - 'L' when Spheres® integrates the lif.xxxxxxf.sp5 file
  - 'O' when Spheres® integrates the opt.xxxxxxf.sp5 file
  -->
  <xsl:template name="GetExchangeSymbol_LIFFE">
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pExchangeSymbol"/>
      <SQL command="select" result="EXCHANGESYMBOL" cache="true">
          <![CDATA[
            select '0' as LEVELSORT,
                  case when ASSETCATEGORY='EquityAsset' then 'O' else 'L' end as EXCHANGESYMBOL
             from dbo.ICEEUPRODUCT
             where CONTRACTSYMBOL=@CONTRACTSYMBOLEXL
               and ORIGINEDATA='BCLEAR'
               and CATEGORY=@CATEGORY
               and case when ASSETCATEGORY='EquityAsset' then 'O' else 'L' end =@EXCHANGESYMBOLIN 
           union all
            select '1' as LEVELSORT,
                   case when ASSETCATEGORY='EquityAsset' then 'O' else 'L' end as EXCHANGESYMBOL
            from dbo.ICEEUPRODUCT
            where CONTRACTSYMBOL=@CONTRACTSYMBOLEXL
              and ORIGINEDATA='SPAN'
              and CATEGORY=@CATEGORY
              and case when ASSETCATEGORY='EquityAsset' then 'O' else 'L' end =@EXCHANGESYMBOLIN
            union all
            select '2' as LEVELSORT,
                   case when ASSETCATEGORY='EquityAsset' then 'O' else 'L' end as EXCHANGESYMBOL
             from dbo.ICEEUPRODUCT
             where CONTRACTSYMBOL=@CONTRACTSYMBOLEXL
               and ORIGINEDATA='BCLEAR'
               and CATEGORY=@CATEGORY
           union all
            select '3' as LEVELSORT,
                   case when ASSETCATEGORY='EquityAsset' then 'O' else 'L' end as EXCHANGESYMBOL
            from dbo.ICEEUPRODUCT
            where CONTRACTSYMBOL=@CONTRACTSYMBOLEXL
              and ORIGINEDATA='SPAN'
              and CATEGORY=@CATEGORY
           union all
            select '4' as LEVELSORT,
                   'L' as EXCHANGESYMBOL
            from DUAL
           order by LEVELSORT
          ]]>

          <Param name="CONTRACTSYMBOLEXL" datatype="string">
            <xsl:value-of select="$pContractSymbol"/>
          </Param>
          <Param name="CATEGORY" datatype="string">
            <xsl:value-of select="$pCategory"/>
          </Param>
          <Param name="EXCHANGESYMBOLIN" datatype="string">
          <xsl:value-of select="$pExchangeSymbol"/>
          </Param>

      </SQL>
           
  </xsl:template>

  <!-- PM 20140512 [19970][19259] Creation Template : GetCurrency_LIFFE -->
  <!-- FI/PM 20141002 [19669] add param pExchangeSymbol -->
  <xsl:template name="GetCurrency_LIFFE">
    <xsl:param name="pContractSymbol"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pExchangeSymbol"/>

    <xsl:call-template name="ovrSQLGetDerivativeContractDefaultValue">
      <xsl:with-param name="pResult" select="'IDC_PRICE'"/>
      <xsl:with-param name="pExtSQLFilterNames" select="concat('CONTRACTSYMBOLEXL',',','CATEGORY',',','EXCHANGESYMBOLEXL')"/>
      <xsl:with-param name="pExtSQLFilterValues" select="concat($pContractSymbol,',',$pCategory,',',$pExchangeSymbol)"/>
    </xsl:call-template>
  </xsl:template>
  
  <!-- ================================================== -->
  <!--        rowStreamCommon_LIFFE                       -->
  <!-- ================================================== -->
  <xsl:template name="rowStreamCommon_LIFFE">
    <xsl:param name="pExchangeSymbol"/>

    <!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~ -->
    <!-- Récupération des variables -->
    <!-- ~~~~~~~~~~~~~~~~~~~~~~~~~~ -->
    
    <!-- ISO10383 -->
    <!-- PL 20141002 [19669] XLIF replaced by IFLL or IFLO -->
    <!--<xsl:variable name="vISO10383">XLIF</xsl:variable>-->
    <xsl:variable name="vISO10383" select="concat('IFL',$pExchangeSymbol)"/>

    <!-- ContractSymbol -->
    <xsl:variable name="vContractSymbol">
      <xsl:value-of select="data[@name='C']"/>
    </xsl:variable>

    <!-- FL/RD 201400425 [19648] Template call: GetExchangeSymbol_LIFFE -->
    <!-- Category -->
    <xsl:variable name="vCategory">
      <xsl:value-of select="data[@name='CT']"/>
    </xsl:variable>

    <!-- ExchangeSymbol -->
    <!-- FL/RD 20140326 [19648] Template call: GetExchangeSymbol_LIFFE -->
    <!--<xsl:variable name="vExchangeSymbol">
      <xsl:value-of select="$pExchangeSymbol"/>
    </xsl:variable>-->
    <!-- FL/RD 20140425 [19648] Adding the parameter pCategory -->
    <!-- FI/PM 20141002 [19669] call template with param pExchangeSymbol -->
    <xsl:variable name="vParamExchangeSymbol">
      <Param name="EXCHANGESYMBOL" datatype="string">
        <xsl:call-template name="GetExchangeSymbol_LIFFE">
          <xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
          <xsl:with-param name="pCategory" select="$vCategory"/>
          <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
        </xsl:call-template>
      </Param>
    </xsl:variable>

    <!-- Currency -->
    <xsl:variable name="vCurrency">
      <xsl:value-of select="data[@name='Cur']"/>
    </xsl:variable>

    <!-- FL/RD 20140425 [19648] Code moved above -->
    <!-- Category -->
    <!--<xsl:variable name="vCategory">
      <xsl:value-of select="data[@name='CT']"/>
    </xsl:variable>-->

    <!-- FutValuationMethod -->
    <xsl:variable name="vFutValuationMethod">
      <xsl:choose>
        <xsl:when test="data[@name='SM'] = '1'">EQTY</xsl:when>
        <xsl:when test="data[@name='SM'] = '2'">FUT</xsl:when>
      </xsl:choose>
    </xsl:variable>
   
    <!-- AssignmentMethod -->
    <xsl:variable name="vAssignmentMethod">
      <xsl:choose>
        <xsl:when test="$vCategory = 'F'">null</xsl:when>
        <xsl:when test="$vCategory = 'O'">R</xsl:when>
      </xsl:choose>
    </xsl:variable>

    <!-- InstrumentIdentifier -->
    <xsl:variable name="vInstrumentIdentifier">
      <xsl:call-template name="InstrumentIdentifier">
        <xsl:with-param name="pCategory" select="$vCategory"/>
      </xsl:call-template>
    </xsl:variable>

    <!-- StrikePriceDecimalLocator -->
    <!-- Voir Ticket [33119] SD : Strike Denominator
          Exemples: SD=1000 -> vStrikePriceDecimalLocator=3
                    SD=100  -> vStrikePriceDecimalLocator=2
                    SD=10   -> vStrikePriceDecimalLocator=1
                      ... -->
    <xsl:variable name="vStrikePriceDecimalLocator">
      <xsl:value-of select="number(string-length(data[@name='SD'])) - 1"/>
    </xsl:variable>

    <!-- ~~~~~~~~~~~~~~~~~ -->
    <!-- Génération du SQL -->
    <!-- ~~~~~~~~~~~~~~~~~ -->

    <xsl:variable name="vSettltMethod">
      <xsl:call-template name="GetSettltMethod_LIFFE">
        <xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
        <xsl:with-param name="pCategory" select="$vCategory"/>
        <xsl:with-param name="pContractDescription" select="data[@name='Desc']"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vAssetCategory">
      <xsl:call-template name="GetAssetCategory_LIFFE">
        <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
        <xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
        <xsl:with-param name="pCategory" select="$vCategory"/>
        <xsl:with-param name="pContractDescription" select="data[@name='Desc']"/>
      </xsl:call-template>
    </xsl:variable>

    <xsl:variable name="vUnderlyingAsset">
      <xsl:call-template name="GetUnderlyingAsset_LIFFE">
        <xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
        <xsl:with-param name="pCategory" select="$vCategory"/>
        <xsl:with-param name="pContractDescription" select="data[@name='Desc']"/>
      </xsl:call-template>
    </xsl:variable>
    
    <!-- We insert all the DCs present in the SPAN file in the ICE-EUPRODUCT table with OrigineData = 'SPAN' because some
            DC exist in the LIFFE file but not in the BCLEAR file.
            (The DCs concerned belong to the LIFFE Financials activity)
                Examples of DC LIFFE Financials activity:
                  - I  Three Month Euro
                  - L  THREE MONTH STERLING
                  - O  5Yr Euro Swapnote
                  - P  10Yr Euro Swapnote
                  
            - WARNING: The data concerning TickSize, TickValue and ContractMultiplier from the SPAN file are false
                          when the Multiplier is less than 1. They correspond to the DCs of Category Equity.
                         In this case, the information from the BCLEAR file is used.
                          or the TickSize, TickValue and Multiplier data is always correct) -->
    <xsl:call-template name="GenerateInsertICEEUPRODUCT">
      <xsl:with-param name="pOrigineData" select="'SPAN'"/>
      <xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
      <xsl:with-param name="pCategory" select="$vCategory"/>
      <xsl:with-param name="pAssetCategory" select="$vAssetCategory"/>
      <xsl:with-param name="pSettltMethod" select="$vSettltMethod"/>
      <!-- Tick Size = Minimum Price Fluctuation / Tick Denominator -->
      <xsl:with-param name="pTickSize" select="data[@name='MPF'] div data[@name='TD']"/>
      <!-- Tick Value = Minimum Price Fluctuation * Tick Value -->
      <xsl:with-param name="pTickValue" select="data[@name='MPF'] * data[@name='TV']"/>
      <!-- Contract Multiplier = Tick Denominator * Tick Value -->
      <xsl:with-param name="pContractMultiplier" select="data[@name='TD'] * data[@name='TV']"/>
      <xsl:with-param name="pContractType" select="'STD'"/>
      <xsl:with-param name="PUnderlyingGroup" select="'F'"/>
      <xsl:with-param name="PUnderlyingAsset" select="$vUnderlyingAsset"/>
      <xsl:with-param name="pCurrency" select="$vCurrency"/>
    </xsl:call-template>

    <xsl:variable name="vDescription">
      <!-- Arrangement de DERIVATIVECONTRACT.DESCRIPTION -->
      <xsl:call-template name="SetDCDescription_LIFFE">
        <xsl:with-param name="pContractCode" select="$vContractSymbol"/>
        <xsl:with-param name="pCategory" select="$vCategory"/>
        <xsl:with-param name="pContractDescription" select="data[@name='Desc']"/>
      </xsl:call-template>
    </xsl:variable>
    <xsl:variable name="vExtSQLFilterNames"  select="concat('LIFFE_SPECIAL',',','CATEGORY')"/>
    <xsl:variable name="vExtSQLFilterValues" select="concat('KEY_OP',',',$vCategory)"/>

    <!--PM 20140512 [19970][19259] Take the currency in ICEEUPRODUCT and not systematically that of SPAN -->
    <!-- FI/PM 20141002 [19669] call template with param pExchangeSymbol -->
    <xsl:variable name="vReadCurrency">
      <xsl:call-template name="GetCurrency_LIFFE">
        <xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
        <xsl:with-param name="pCategory" select="$vCategory"/>
        <xsl:with-param name ="pExchangeSymbol" select="$pExchangeSymbol"/>
      </xsl:call-template>
    </xsl:variable>

    <!-- Création des DC -->
    <xsl:call-template name="SQLTableDERIVATIVECONTRACT">
      <xsl:with-param name="pISO10383" select="$vISO10383"/>

      <!--FL 20140220 [19648] - GLOP -->
      <!--<xsl:with-param name="pExchangeSymbol" select="$vExchangeSymbol"/>-->
      <xsl:with-param name="pExchangeSymbol" select="$pExchangeSymbol"/>
      <xsl:with-param name="pParamExchangeSymbol">
        <xsl:copy-of select="$vParamExchangeSymbol"/>
      </xsl:with-param>      
      <xsl:with-param name="pContractSymbol" select="$vContractSymbol"/>
      <xsl:with-param name="pDerivativeContractIdentifier" select="$gAutomaticCompute"/>
      <xsl:with-param name="pContractDisplayName" select="$gAutomaticCompute"/>
      <xsl:with-param name="pDescription" select="$vDescription"/>
      <xsl:with-param name="pInstrumentIdentifier" select="$vInstrumentIdentifier"/>
      <!--PM 20140512 [19970][19259] Take the currency in ICEEUPRODUCT and not systematically that of SPAN -->
      <!--<xsl:with-param name="pCurrency" select="$vCurrency"/>-->
      <xsl:with-param name="pCurrency" select="$vReadCurrency"/>
      <xsl:with-param name="pCategory" select="$vCategory"/>
      <xsl:with-param name="pFutValuationMethod" select="$vFutValuationMethod"/>
      <xsl:with-param name="pAssignmentMethod" select="$vAssignmentMethod"/>
      <xsl:with-param name="pStrikeDecLocator" select="$vStrikePriceDecimalLocator"/>
      <xsl:with-param name="pUseDerivativeContractDefaultValue" select="true()"/>
      <!-- RD/FL 20140619[20113] Regardless of the value of pContractFactor,
              if pUseFactorDefaultValue = true, we call the ovrSQLGetDerivativeContractDefaultValue template to load the value of the FACTOR column -->
      <xsl:with-param name="pUseFactorDefaultValue" select="true()"/>
      <xsl:with-param name="pExtSQLFilterNames"  select="$vExtSQLFilterNames"/>
      <xsl:with-param name="pExtSQLFilterValues" select="$vExtSQLFilterValues"/>
      <xsl:with-param name="pDerivativeContractIsAutoSetting" select="$gTrue"/>
      <!--PM 20140515 [19970][19259] Addition of pNominalCurrency to continue taking the currency of the SPAN file for the currency of the nominal -->
      <xsl:with-param name="pNominalCurrency" select="$vCurrency"/>    
      <!-- FL/RD 20220105 [25920] add Check AssetCategory -->
      <xsl:with-param name="pIsCheckAssetCategory" select="$gTrue"/>
    </xsl:call-template>
  </xsl:template>

  <!-- ================================================== -->
  <!--        ovrSQLGetDerivativeContractDefaultValue     -->
  <!-- ================================================== -->
  <!-- FI 20131120 [19216] add column CONTRACTTYPE-->
  <!-- FI 20131121 [19216] CONTRACTTYPE = 'STD' if the DC is not present in ICEEUPRODUCT (BCLEAR)-->
  <!-- FI 20131129 [19284] add FINALSETTLTSIDE (FINALSETTLTSIDE always returns 'OfficialClose' on DC options -->
  <!-- FL 20140220 [19648] add UNDERLYINGGROUP & UNDERLYINGASSET(Always returns the values ​​contained in the ICEEUPRODUCT table) -->
  <!-- PM 20140512 [19970][19259] add IDC (Always returns the values ​​contained in the ICE PRODUCT table) -->
  <!-- FI/PM 20141002 [19669] taking into consideration EXCHANGESYMBOLEXL (cas 0 et cas 1) -->
  <!-- FL/PLA 20170420 [23064] add column PHYSETTLTAMOUNT -->
  <xsl:template name="ovrSQLGetDerivativeContractDefaultValue">
    <xsl:param name="pResult"/>
    <xsl:param name="pExtSQLFilterValues"/>
    <xsl:param name="pExtSQLFilterNames"/>
    <xsl:choose>
      <xsl:when test="$pResult='SETTLTMETHOD' or
                      $pResult='PHYSETTLTAMOUNT' or
                      $pResult='EXERCISESTYLE' or
                      $pResult='CONTRACTMULTIPLIER' or
                      $pResult='FACTOR' or
                      $pResult='MINPRICEINCR' or
                      $pResult='MINPRICEINCRAMOUNT' or
                      $pResult='ASSETCATEGORY' or
                      $pResult='IDASSET_UNL' or
                      $pResult='IDMATURITYRULE' or
                      $pResult='CONTRACTTYPE' or 
                      $pResult='FINALSETTLTSIDE' or
                      $pResult='UNDERLYINGGROUP' or
                      $pResult='UNDERLYINGASSET' or
                      $pResult='IDC_PRICE'">
        <!-- We look for the missing data when importing DCs in ICEEUPRODUCT the contract
              according to its Symbol and its Category.
              First of all the data coming from the file 'BCLEAR'
              Then those coming from the LIFFE file (ORIGINEDATA = 'SPAN') -->
        <SQL command="select" result="{$pResult}" cache="true">
          <![CDATA[
                    
          select '0' as LEVELSORT,
            SETTLTMETHOD, case when SETTLTMETHOD = 'C' then 'NA' else 'None' end as PHYSETTLTAMOUNT, EXERCISESTYLE, CONTRACTMULTIPLIER,
            FACTOR, MINPRICEINCR, MINPRICEINCRAMOUNT,
            ASSETCATEGORY, IDASSET_UNL, IDMATURITYRULE, CONTRACTTYPE,
            case when CATEGORY='O' then 'OfficialClose' else null end as FINALSETTLTSIDE,
            UNDERLYINGGROUP, UNDERLYINGASSET, IDC_PRICE
          from dbo.ICEEUPRODUCT
          where CONTRACTSYMBOL=@CONTRACTSYMBOLEXL
            and CATEGORY=@CATEGORY
            and ORIGINEDATA='BCLEAR'
            and case when ASSETCATEGORY='EquityAsset' then 'O' else 'L' end =@EXCHANGESYMBOLEXL
          union all
          select '1' as LEVELSORT,
            SETTLTMETHOD,  case when SETTLTMETHOD = 'C' then 'NA' else 'None' end as PHYSETTLTAMOUNT, EXERCISESTYLE, CONTRACTMULTIPLIER,
            FACTOR, MINPRICEINCR, MINPRICEINCRAMOUNT,
            ASSETCATEGORY, IDASSET_UNL, IDMATURITYRULE, CONTRACTTYPE,
            case when CATEGORY='O' then 'OfficialClose' else null end as FINALSETTLTSIDE,
            UNDERLYINGGROUP, UNDERLYINGASSET, IDC_PRICE
          from dbo.ICEEUPRODUCT
          where CONTRACTSYMBOL=@CONTRACTSYMBOLEXL
            and CATEGORY=@CATEGORY
            and ORIGINEDATA='SPAN'
            and case when ASSETCATEGORY='EquityAsset' then 'O' else 'L' end =@EXCHANGESYMBOLEXL
          union all          
          select '2' as LEVELSORT,
            SETTLTMETHOD, case when SETTLTMETHOD = 'C' then 'NA' else 'None' end as PHYSETTLTAMOUNT, EXERCISESTYLE, CONTRACTMULTIPLIER,
            FACTOR, MINPRICEINCR, MINPRICEINCRAMOUNT,
            ASSETCATEGORY, IDASSET_UNL, IDMATURITYRULE, CONTRACTTYPE,
            case when CATEGORY='O' then 'OfficialClose' else null end as FINALSETTLTSIDE,
            UNDERLYINGGROUP, UNDERLYINGASSET, IDC_PRICE
          from dbo.ICEEUPRODUCT
          where CONTRACTSYMBOL=@CONTRACTSYMBOLEXL
            and CATEGORY=@CATEGORY
            and ORIGINEDATA='BCLEAR'
          union all
          select '3' as LEVELSORT,
            SETTLTMETHOD, case when SETTLTMETHOD = 'C' then 'NA' else 'None' end as PHYSETTLTAMOUNT, EXERCISESTYLE, CONTRACTMULTIPLIER,
            FACTOR, MINPRICEINCR, MINPRICEINCRAMOUNT,
            ASSETCATEGORY, IDASSET_UNL, IDMATURITYRULE, CONTRACTTYPE,
            case when CATEGORY='O' then 'OfficialClose' else null end as FINALSETTLTSIDE,
            UNDERLYINGGROUP, UNDERLYINGASSET, IDC_PRICE
          from dbo.ICEEUPRODUCT
          where CONTRACTSYMBOL=@CONTRACTSYMBOLEXL
            and CATEGORY=@CATEGORY
            and ORIGINEDATA='SPAN'
          union all
          select '4' as LEVELSORT,
            'C' as SETTLTMETHOD,  'NA' as PHYSETTLTAMOUNT, '1' as EXERCISESTYLE, null as CONTRACTMULTIPLIER,
            null as FACTOR, null as MINPRICEINCR, null as MINPRICEINCRAMOUNT,
            null as ASSETCATEGORY, null as IDASSET_UNL, null as IDMATURITYRULE, 'STD' as CONTRACTTYPE,
            case when @CATEGORY='O' then 'OfficialClose' else null end as FINALSETTLTSIDE,
            'F' as UNDERLYINGGROUP, null as UNDERLYINGASSET, null as IDC_PRICE
          from DUAL
          order by LEVELSORT
          ]]>
          <xsl:call-template name="ParamNodesBuilder">
            <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
            <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
          </xsl:call-template>
        </SQL>
      </xsl:when>

      <xsl:when test="$pResult='IDDC_UNL'">
        <!--  Contract Symbol of the Option / Future   Contract Symbol of the underlying Future
              =================================================================================
              K, K2, K3, K4, K5                        I
              M, M2, M3, M4, M5                        L
              SY1, SY2, SY3, SY4, SY5                  SO3
              In other cases, the contract symbol of the underlying future is the same as that of the option.
              The Options with ContractSymbol K .. or M .. correspond to the MIDCURVE Options..
        -->
        <!-- BD 20130520: Call template SQLDTENABLEDDTDISABLED o check the validity of the selected DC -->
        <!-- FL 20140619[20113]: For all Futures and Futures Otpions on the LIFFE ORIGINEDATA is
                                  equal to 'SPAN' and not to 'BCLEAR' -->
        <!-- PL 20141002 [19669] XLIF replaced by IFLL -->
        <SQL command="select" result="IDDC" cache="true">
          <![CDATA[
          select dc.IDDC
          from dbo.DERIVATIVECONTRACT dc
          inner join dbo.MARKET m on (m.IDM=dc.IDM) and (m.EXCHANGESYMBOL='L') and (m.ISO10383_ALPHA4='IFLL')
          inner join dbo.ICEEUPRODUCT ice on (ice.CONTRACTSYMBOL=@CONTRACTSYMBOLEXL) and (ice.ORIGINEDATA='SPAN') and (ice.ASSETCATEGORY='Future')
          where dc.CATEGORY='F'
            and dc.CONTRACTSYMBOL=case 
                 when @CONTRACTSYMBOLEXL in ('K','K2','K3','K4','K5') then 'I'
                 when @CONTRACTSYMBOLEXL in ('M','M2','M3','M4','M5') then 'L'
                 when @CONTRACTSYMBOLEXL in ('SY1','SY2','SY3','SY4','SY5') then 'SO3'
                 else @CONTRACTSYMBOLEXL end
          ]]>
          <xsl:call-template name="SQLDTENABLEDDTDISABLED">
            <xsl:with-param name="pTable" select="'dc'"/>
          </xsl:call-template>
          <xsl:call-template name="ParamNodesBuilder">
            <xsl:with-param name="pValues" select="$pExtSQLFilterValues"/>
            <xsl:with-param name="pNames" select="$pExtSQLFilterNames"/>
          </xsl:call-template>
          <Param name="DT" datatype="date">
            <xsl:value-of select="/iotask/parameters/parameter[@id='DTBUSINESS']"/>
          </Param>
        </SQL>
      </xsl:when>

      <!-- BD 20130523 We use Tick Denominator to inform PRICEDECLOCATOR - See Ticket 33119
                    TD : Tick Denominator
            Exemples: TD=1000 -> PRICEDECLOCATOR=3
                      TD=100  -> PRICEDECLOCATOR=2
                      TD=10   -> PRICEDECLOCATOR=1
                      ... -->
      <xsl:when test="$pResult='PRICEDECLOCATOR'">
        <xsl:value-of select="number(string-length(data[@name='TD'])) - 1"/>
      </xsl:when>

      <xsl:otherwise>null</xsl:otherwise>

    </xsl:choose>

  </xsl:template>

  <!-- ============================================== -->
  <!--           SetDCDescription_LIFFE               -->
  <!-- ============================================== -->
  <xsl:template name="SetDCDescription_LIFFE">
    <xsl:param name="pContractCode"/>
    <xsl:param name="pCategory"/>
    <xsl:param name="pContractDescription"/>
    <!-- ContractDescription is composed as follows: "Description - ExerciseStyle SettltMethod Category"
                                                    Example: "Sanofi - Euro Cash F"
         However, depending on the length of the description ("Sanofi" in the example),
          only the first letter of ExerciseStyle or SettltMethod is present
          (hence the verification of the presence of a "P" or an "E" after the dash ("-") of the description -->

    <!-- We get the description after the dash (-) -->
    <xsl:variable name="DescriptionAfterDash">
      <xsl:value-of select="substring-after($pContractDescription,'-')"/>
    </xsl:variable>

    <!-- We get the description before the dash (-), if there is one -->
    <xsl:variable name="DescriptionBeforeDash">
      <xsl:choose>
        <xsl:when test="contains($pContractDescription,'-')">
          <xsl:value-of select="substring-before($pContractDescription,'-')"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="$pContractDescription"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="Description">
      <xsl:choose>
        <xsl:when test="contains($DescriptionBeforeDash,'(')">
          <xsl:value-of select="normalize-space(substring-before($DescriptionBeforeDash,'('))"/>
        </xsl:when>
        <xsl:otherwise>
          <xsl:value-of select="normalize-space($DescriptionBeforeDash)"/>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="FutOrOpt">
      <xsl:choose>
        <xsl:when test="$pCategory='F'">Fut</xsl:when>
        <xsl:when test="$pCategory='O'">Opt</xsl:when>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="CashOrPhys">
      <xsl:choose>
        <xsl:when test="contains($DescriptionAfterDash,'P')">Phys</xsl:when>
        <xsl:otherwise>Cash</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:variable name="AmOrEu">
      <xsl:choose>
        <xsl:when test="contains($DescriptionAfterDash,'E')">Eu</xsl:when>
        <xsl:otherwise>Am</xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:value-of select="concat(
                            $pContractCode,' ',
                            $FutOrOpt,' ',
                            '(',normalize-space($Description),') ',
                            $CashOrPhys,' ',
                            $AmOrEu
                          )"/>
  </xsl:template>

</xsl:stylesheet>