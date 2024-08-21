using System;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using EFS.ACommon;

namespace EFS.Common.Web
{
	/// <summary>
	/// Description résumée de JavaScript.
	/// </summary>
	/// EG 20234429 [WI756] Spheres Core : Refactoring Code Analysis
	public sealed class XMLJavaScript
	{
		public XMLJavaScript()
		{
			//
			// TODO : ajoutez ici la logique du constructeur
			//
		}
		public static string JSString(string pString)
		{
			return "'"+ pString.Replace("'", "''") + "'";
		}


		private static void StringSplit(string str, ref string[] astr)
		{
			if (str == null)
				return;
			string delimStr = ";";
			if (!(str.IndexOf(delimStr)>0))
			{
				astr = new string[1];
				astr[0] = str;
			}
			else
			{
				char [] delimiter = delimStr.ToCharArray();
				astr = str.Split(delimiter);
			}
		}	

		#region Méthodes spécifiques d'interprétation des paramètres par f° Javascript
		#region Différentes fonctions JavaScript utilisables dans les fichiers XML
		/// <summary>
		/// 
		/// Différentes fonctions JavaScript utilisables dans les fichiers XML
		/// 
		/// *********************************************
		/// *Arborescence XML pour l'appel de fonctions:*
		/// *********************************************
		/// <Referentials>
		///		<Referential>
		///			<Column>
		///				<JavaScript>
		///					<Script attribut="xxxxx" name="EFS_xxxxx" control="xxxx" data="xxxx" condition="xxxx"></Script>
		///				</JavaSrcipt>
		///			</Column>
		///		</Referential>
		///	</Referentials>
		/// 
		///***********************
		///*Fonctions disponibles*
		///***********************
		///EFS_Enabled
		///EFS_Disabled
		///EFS_Copy
		///EFS_Set
		///EFS_Reset
		///EFS_Upper
		///EFS_Lower
		///EFS_1stUpper
		///EFS_Status
		///EFS_StatusHelp	
		///
		///**************************
		///*Liste des attributs XML:*
		///**************************
		///attribut						:	Correspond a l'attribut sur lequel on va fixer le JavaScript : 1 ou N selon fonction (attribut="xxxx;yyyy;zzzz;...")</param>
		///name							:	Indique le nom de la fonction EFS_JavaScript à utiliser : 1 seul par fonction</param>
		///control						:	Donne le nom(ou ID) du control affecté par le script : 1 ou N selon fonction (control="xxxx;yyyy;zzzz;...")</param>
		///data (ou resdata)			:	Valeur à renseigner, spécifique à la fonction appelée : 1 ou N selon fonction (data="xxxx;yyyy;zzzz;...")
		///									Pour faire appel à des valeurs traduites, utiliser l'attribut 'resdata' au lieu de 'data'</param>
		///condition (ou rescondition)	:	Valeur à renseigner, spécifique à la fonction appelée : 1 ou N selon fonction (condition="xxxx;yyyy;zzzz;...")
		///									Pour faire appel à des valeurs traduites, utiliser l'attribut 'rescondition' au lieu de 'condition'</param>
		/// 
		///****************************
		///*Description des fonctions:*
		///****************************
		///EFS_Enabled		Fonction permettant de rendre éditable un(ou N) control(s) en fonction de la valeur du control appelant
		///									attributs:	ATTRIBUT(1>N): attribut(s) sur le(s)quel(s) la fonction est appelée
		///												CONTROL(1>N): control(s) affecté(s) par le changement d'état (enabled//disabled) 
		///												CONDITION (1>N): valeur(s) testée(s) pour l'affectation de l'état 
		///
		///EFS_Disabled		cf. EFS_Enabled
		/// 
		///EFS_Copy			Fonction de copie de la valeur d'un champ dans un ou plusieurs autres champs
		///									attributs:	ATTRIBUT(1>N): attribut(s) sur le(s)quel(s) la fonction est appelée
		///												CONTROL(1>N): control(s) cible(s) pour la copie 
		///												CONDITION (0>1): new(default) | append | replace 
		///
		///EFS_Set			Fonction d'affectation de valeur à un(plusieurs) control(s)
		///									attributs:	ATTRIBUT(1>N): attribut(s) sur le(s)quel(s) la fonction est appelée
		///												CONTROL(1>N): control(s) ciblé(s)
		///												DATA (1>N): valeur(s) pour le(s) control(s)												
		///												<non obligatoire> CONDITION (1>N): valeur(s) testée(s) pour l'affectation de la valeur
		///									Note: si N controls et 1 seul SVALUE, tous les controls sont affecter par l'unique valeur de SVALUE
		///
		///EFS_Reset		Fonction de reset d'un(plusieurs) control(s)
		///									attributs:	ATTRIBUT(1>N): attribut(s) sur le(s)quel(s) la fonction est appelée
		///												CONTROL(1>N): control(s) ciblé(s)
		///												<non obligatoire> CONDITION (1>N): valeur(s) testée(s) pour la validation du reset
		///
		///EFS_Upper		Fonction de UPPER sur la valeur du control sur lequel la fonction est branchée.
		///									attributs:	ATTRIBUT(1>N): attribut(s) sur le(s)quel(s) la fonction est appelée		///											
		///
		///EFS_Lower		cf. EFS_Upper
		///
		///EFS_1stUpper		cf. EFS_Upper
		///
		///EFS_Status		Fonction qui permet de renseigner le 'status' de la fenêtre Explorateur
		///									attributs:	ATTRIBUT(1>N): attribut(s) sur le(s)quel(s) la fonction est appelée
		///												DATA (1>1): valeur a mettre dans le status
		///
		///EFS_StatusHelp	Fonction qui permet de renseigner le 'status' de la fenêtre Explorateur
		///									Le principe est le même que EFS_Status excepté que le status est renseigné sur le onfocus et remis a zero sur le onblur automatiquement
		///									attributs:	DATA (1>1): valeur a mettre dans le status
		/// </summary>
		#endregion

		public static void ArrayGetRessource(ref string[] array)
		{
			for(int i=0;i<array.Length;i++)
			{
				array[i]=Ressource.GetString(array[i],"~" + array[i] + "~");
			}
		}
		public static void Interpret(JavaScript.JavaScriptScript jss)
		{			
			StringSplit(jss.attribut,ref jss.aAttribut);
			StringSplit(jss.control,ref jss.aControl);
			if (jss.resdata!=null)
			{
				StringSplit(jss.resdata,ref jss.aData);
				ArrayGetRessource(ref jss.aData);
			}
			else
				StringSplit(jss.data,ref jss.aData);
			if (jss.rescondition!=null)
			{
				StringSplit(jss.rescondition,ref jss.aCondition);
				ArrayGetRessource(ref jss.aCondition);
			}
			else
				StringSplit(jss.condition,ref jss.aCondition);
            
			if(JavaScript.IsScriptTypeEnabledDisabled(jss.name))
				//InterpretEnabledDisabled(jss); l'appel s'effectue manuellement a partir du script d'appel : afin de renseigner les controlID
				return;
            else if (JavaScript.IsScriptTypeEFS_Copy(jss.name))
				//InterpretCopy(jss); l'appel s'effectue manuellement a partir du script d'appel : afin de renseigner les controlID
				return;
            else if (JavaScript.IsScriptTypeEFS_Set(jss.name) || JavaScript.IsScriptTypeEFS_Reset(jss.name))
			{
				if (!(JavaScript.IsScriptTypeEFS_Reset(jss.name)))
				{
					if (jss.aControl.Length > jss.aData.Length)
					{
						string tempvalue = jss.aData[0];
						jss.aData = new String[jss.aControl.Length];
						for (int i=0;i<jss.aControl.Length;i++)
						{
							jss.aData[i]=tempvalue;
						}
					}
				}
				//InterpretSet(jss); l'appel s'effectue manuellement a partir du script d'appel : afin de renseigner les controlID
				return;
			}
            else if ((JavaScript.IsScriptTypeEFS_Upper(jss.name)) || JavaScript.IsScriptTypeEFS_Lower(jss.name) || JavaScript.IsScriptTypeEFS_1stUpper(jss.name))
			{
				InterpretUpperLower(jss);
				return;
			}
			else if (JavaScript.IsScriptTypeEFS_Status(jss.name))
			{
				InterpretSetStatus(jss);
				return;
			}
            else if (JavaScript.IsScriptTypeEFS_StatusHelp(jss.name))
			{
				InterpretSetStatusHelp(jss);
				return;
			}
			else
				//Interpretxxxxxxx(jss);
				return;
		}


		#region Aide dans la statusBar de l'explorateur
		/// <summary>
		/// 
		/// Fonction d'aide
		/// 
		/// **************
		/// *Arborescence*
		/// ************** 
		/// <Referentials>
		///		<Referential>
		///			<Column>
		///				<Help>RessourceDuTexteAide</Help>
		///			</Column>
		///		</Referential>
		///	</Referentials>
		///	
		///	****************
		///	*Fonctionnement*
		///	****************
		///	Alimente la statusBar de l'explorateur avec la ressource mentionnée
		///	
		/// </summary>
		public static void SetHelp(Control ctrlRef, string strHelp)
		{
			string script;
			script="setStatus('" + Ressource.GetString(strHelp,"~" + strHelp + "~") +"');";
			AddScriptToCtrl(ctrlRef,"onfocus",script);
			script="setStatus(' ');";
			AddScriptToCtrl(ctrlRef,"onblur",script);
		}
		#endregion

		#region EnabledDisabled
		public static void InterpretEnabledDisabled(JavaScript.JavaScriptScript jss)
		{
			jss.Jscript= new string[jss.aControl.Length * jss.aCondition.Length];
            for (int i=0;i<jss.aControl.Length;i++)
			{
                long indice = 0;
                for (int j=0;j<jss.aCondition.Length;j++)
				{
					jss.Jscript[i]+="EnabledDisabled(this,"+ ((jss.aControl[i]!="this")?"'" + jss.aControl[i] + "'":"this.id" ) + ",'" + jss.aCondition[j] + "'," + (JavaScript.IsScriptTypeEFS_Enabled(jss.name)?"true":"false") + ",";
					indice++;
				}
				jss.Jscript[i]+= "false)";
				if (!(indice==1))
				{
					for (int j=1;j<indice;j++)
					{
						jss.Jscript[i]+= ")";
					}			
				}
				jss.Jscript[i]+= ";";
			}
		}
		#endregion

        #region InterpretApplyOffset
        public static void InterpretApplyOffset(JavaScript.JavaScriptScript jss)
        {
            jss.Jscript= new string[1];
            if ((jss.condition != "append") && (jss.condition != "replace"))
                jss.condition = "new";

            StringBuilder sb = new StringBuilder();
            string arg = string.Empty;
            for (int i = 0; i < jss.aControl.Length; i++)
                arg+= jss.aControl[i] + ";";

            sb.AppendFormat("javascript:__doPostBack('{0}','{1}');", jss.name, arg);
            jss.Jscript[0] = sb.ToString();
            jss.Jscript[0] += ";";
        }
        #endregion

		#region Copy
		public static void InterpretCopy(JavaScript.JavaScriptScript jss)
		{
			jss.Jscript= new string[jss.aControl.Length];
			if ((jss.condition != "append")&&(jss.condition != "replace"))
				jss.condition = "new";
			for (int i=0;i<jss.aControl.Length;i++)
			{
				jss.Jscript[i]="Copy(this,'" + jss.aControl[i] + "','" + jss.condition + "')";				
				jss.Jscript[i]+= ";";
			}
		}
		#endregion
		#region Set
		public static void InterpretSet(JavaScript.JavaScriptScript jss)
		{
            jss.Jscript = new string[jss.aControl.Length];
            bool isReset;
            if (jss.aCondition == null)
            {
                for (int i = 0; i < jss.aControl.Length; i++)
                {
                    isReset = JavaScript.IsScriptTypeEFS_Reset(jss.name);
                    jss.Jscript[i] = (isReset ? "Reset" : "Set");
                    jss.Jscript[i] += "(";
                    //Name of control to set or reset
                    jss.Jscript[i] += ((jss.aControl[i] == "this") ? "this.id" : "'" + jss.aControl[i] + "', ");
                    if (!isReset)
                        //Value to set
                        jss.Jscript[i] += "'" + jss.aData[i] + "'";
                    //Condition
                    jss.Jscript[i] += ", true);";
                }
            }
            else
            {
                for (int i = 0; i < jss.aControl.Length; i++)
                {
                    isReset = JavaScript.IsScriptTypeEFS_Reset(jss.name);
                    jss.Jscript[i] = (isReset ? "Reset" : "Set");
                    jss.Jscript[i] += "(";
                    //Name of control to set or reset
                    jss.Jscript[i] += ((jss.aControl[i] == "this") ? "this.id" : "'" + jss.aControl[i] + "',");
                    if (!isReset)
                        //Value to set
                        jss.Jscript[i] += "'" + jss.aData[i] + "',";
                    //Condition

                    // 20070702 RD
                    //--- Avant
                    //					for(int j=0;j<jss.aCondition.Length;j++)
                    //					{
                    //						jss.Jscript[i] += "IsEqualValue(this, '" + jss.aCondition[j] + "', ";
                    //					}
                    //					jss.Jscript[i] += "true)";
                    //					if (jss.aCondition.Length > 1)
                    //						jss.Jscript[i].PadRight(jss.aCondition.Length -1, ')');
                    //-----

                    //--- Aprés
                    for (int j = 0; j < jss.aCondition.Length; j++)
                    {
                        jss.Jscript[i] += "IsEqualValue(this, '" + jss.aCondition[j] + "', true)" + JavaScript.JS_OR;
                    }
                    if (jss.Jscript[i].EndsWith(JavaScript.JS_OR))
                        jss.Jscript[i] = jss.Jscript[i].Substring(0, jss.Jscript[i].Length - JavaScript.JS_OR.Length);
                    //----

                    jss.Jscript[i] += ");";
                }
            }
        }
		#endregion
		#region UpperLower
		public static void InterpretUpperLower(JavaScript.JavaScriptScript jss)
		{
			jss.Jscript= new string[1];
            jss.Jscript[0] = "UpperLower(this,'" + ((JavaScript.IsScriptTypeEFS_Upper(jss.name)) ? "upper" : (JavaScript.IsScriptTypeEFS_Lower(jss.name)) ? "lower" : "1stupper") + "')";				
			jss.Jscript[0]+= ";";

		}
		#endregion
		#region setStatus
		public static void InterpretSetStatus(JavaScript.JavaScriptScript jss)
		{
			jss.Jscript= new string[1];
			jss.Jscript[0]="setStatus('" + jss.aData[0] +"')";					
		}
		#endregion
		#region setStatusHelp
		public static void InterpretSetStatusHelp(JavaScript.JavaScriptScript jss)
		{
			jss.aAttribut=new string[2];
			jss.aAttribut[0]= "onfocus";
			jss.aAttribut[1]= "onblur";
			jss.Jscript= new string[2];
			jss.Jscript[0]="setStatus('" + jss.aData[0] +"');";
			jss.Jscript[1]="setStatus(' ');";
		}
		#endregion

		#endregion

		#region AddScriptToCtrl
		public static void AddScriptToCtrl(Control ctrlRef, JavaScript.JavaScriptScript jss)
		{
			if (!JavaScript.IsScriptTypeEFS_StatusHelp(jss.name))
			{
				for (int i=0;i<jss.aAttribut.Length;i++)
				{
					if (jss.Jscript == null)
						return;
					for (int j=0;j<jss.Jscript.Length;j++)
					{
						AddScriptToCtrl(ctrlRef,jss.aAttribut[i],jss.Jscript[j]);
					}
				}
			}
			else
			{
				for (int i=0;i<jss.aAttribut.Length;i++)
				{
					AddScriptToCtrl(ctrlRef,jss.aAttribut[i],jss.Jscript[i]);
				}
				return;
			}
		}

        /// EG 20170918 [23342] suppression WCImgCalendar
		public static void AddScriptToCtrl(Control ctrlRef,string attribut,string script)
		{
			string scriptexistant;
			if (ctrlRef.GetType().Equals(typeof(WCCheckBox2)))
			{
				scriptexistant = ((WCCheckBox2)ctrlRef).Attributes[attribut];
				if (scriptexistant != null)
					script = scriptexistant + script;
				((WCCheckBox2)ctrlRef).Attributes.Add(attribut, script);
			}
			else if (ctrlRef.GetType().Equals(typeof(WCDropDownList2)) ||
				ctrlRef.GetType().Equals(typeof(OptionGroupDropDownList))) // FI 20200305 [XXXXX] Add OptionGroupDropDownList
			{
				scriptexistant = ((WCDropDownList2)ctrlRef).Attributes[attribut];
				if (scriptexistant != null)
					script = scriptexistant + script;
				((WCDropDownList2)ctrlRef).Attributes.Add(attribut, script);
			}
			else if (ctrlRef.GetType().Equals(typeof(WCTextBox)))
			{
				scriptexistant = ((WCTextBox)ctrlRef).Attributes[attribut];
				if (scriptexistant != null)
					script = scriptexistant + script;
				((WCTextBox)ctrlRef).Attributes.Add(attribut, script);
			}
			else if (ctrlRef.GetType().Equals(typeof(WCTextBox2)))
			{
				scriptexistant = ((WCTextBox2)ctrlRef).Attributes[attribut];
				if (scriptexistant != null)
					script = scriptexistant + script;
				((WCTextBox2)ctrlRef).Attributes.Add(attribut, script);
			}
			else
				if (ctrlRef.GetType().Equals(typeof(CheckBox)))
			{
				scriptexistant = ((CheckBox)ctrlRef).Attributes[attribut];
				if (scriptexistant != null)
					script = scriptexistant + script;
				((CheckBox)ctrlRef).Attributes.Add(attribut, script);
			}
			else if (ctrlRef.GetType().Equals(typeof(DropDownList)))
			{
				scriptexistant = ((DropDownList)ctrlRef).Attributes[attribut];
				if (scriptexistant != null)
					script = scriptexistant + script;
				((DropDownList)ctrlRef).Attributes.Add(attribut, script);
			}
			else if (ctrlRef.GetType().Equals(typeof(TextBox)))
			{
				scriptexistant = ((TextBox)ctrlRef).Attributes[attribut];
				if (scriptexistant != null)
					script = scriptexistant + script;
				((TextBox)ctrlRef).Attributes.Add(attribut, script);
			}
			else
				return;
		}
		#endregion
	}
}
