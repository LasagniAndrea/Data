<script type="text/javascript">
//<![CDATA[
	var {--HiddenFieldName--} = (document.getElementsByName("{--HiddenFieldName--}")[0]).value.split(", ");
	for(var index = 0; index < {--HiddenFieldName--}.length; index++)
	{
		if({--HiddenFieldName--}[index] != null && {--HiddenFieldName--}[index] != "")
			HierarchicalGrid_toggleRow(document.getElementsByName({--HiddenFieldName--}[index])[0]);
	}
//]]>	
</script>