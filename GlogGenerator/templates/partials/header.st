header(site,page) ::= <<
<nav id="sub-nav">
	<div class="profilepic">
		<a href="%site.BaseURL%"><img src="%site.BaseURL%joystick.png" alt="Glog" style="width: 160px;" /></a>
	</div>
	<p>
		<a href="%site.BaseURL%index.xml" title="RSS"><img src="%site.BaseURL%feed-icon-28x28.png" /></a>
	</p>
</nav>
<hr>
<nav id="main-nav">
	%navigation(site,page)%
</nav>
<hr>
<div class="disclaimer">
	<p>
		<i>The views and opinions expressed on this website are the author's alone. They are not endorsed by the author's employer, nor do they reflect the views and opinions of said employer or any other affiliated entity.</i>
	</p>
</div>
>>
