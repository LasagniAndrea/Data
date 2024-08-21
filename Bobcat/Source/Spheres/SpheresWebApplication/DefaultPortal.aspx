<%@ Page Language="C#" AutoEventWireup="true" Inherits="DefaultPortal" Codebehind="DefaultPortal.aspx.cs" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Frameset//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-frameset.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <!-- #include file="Includes/T_Head.htm" -->
</head>


<frameset id="global" name="global" rows="55,*" framespacing="0" title="Spheres">
    <frame id="banner" name="banner" src="Banner.aspx" frameborder="0" scrolling="no" noresize="noresize" title="Banner"/>
    <frame id="main" name="main" src="Portal/NewPortal.aspx" frameborder="0" scrolling="auto" noresize="noresize" title="Main"/>
    
    <noframes >
        <body>
            <pre id="p3">
                ================================================================
                INSTRUCTIONS POUR RENSEIGNER CE JEU DE FRAMES BANNER ET CONTENT
                ================================================================
			</pre>
            <pre id="p2">
                ================================================================
                INSTRUCTIONS POUR RENSEIGNER CE JEU DE FRAMES BANNER ET CONTENT
                1. Ajoutez l'URL de votre page src="" pour le frame "banner".
                2. Ajoutez l'URL de votre page src="" pour le frame "contents".
                3. Ajoutez l'URL de votre page src="" pour le frame "main".
                4. Ajoutez un élément BASE target="main" à la section HEAD de votre page 
	               "contents", pour définir "main" comme frame par défaut   
                   dans lequel ses liens afficheront d'autres pages. 
                ================================================================
			</pre>
            <p id="p1">
                Ce jeu de frames HTML affiche plusieurs pages Web. Pour afficher ce jeu de frames,
                utilisez un navigateur Web qui prend en charge HTML 4.0 et version ultérieure.
            </p>
        </body>
    </noframes>
</frameset>
</html>
