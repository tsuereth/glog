function spoiler_toggle(element) {
	var oldClasses = element.className;
	if((" " + oldClasses + " ").indexOf(" spoiler_hidden ") > -1)
	{
		element.className = oldClasses.replace("spoiler_hidden", "spoiler_revealed");
	}
	else
	{
		element.className = oldClasses.replace("spoiler_revealed", "spoiler_hidden");
	}
}
