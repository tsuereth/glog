footer(site,page) ::= <<
<p>
	Copyright &copy; 2004 - %site.BuildDate;format="yyyy"%

    %site.Author%

</p>

<script type="text/javascript">
(function($){
	var docHead = document.getElementsByTagName("head")[0];

	var numImages = 0;
	var numCharts = 0;
	var drawChartCallbacks = [];
	$('.entry-content').each(function(i){
		var _i = i;

	
		$(this).find('img').each(function(){
			++numImages;
			var alt = this.alt;

			$(this).wrap('<a href="'+this.src+'" title="'+alt+'" class="fancybox" data-fancybox="gallery'+_i+'" />');
		});

	
		$(this).find('chart').each(function(){
			++numCharts;
			drawChartCallbacks.push($(this).attr("callback"));
		});
	});

	
	if(numImages > 0) {
		var js = document.createElement("script");
		js.src = "%site.BaseURL%js/jquery.fancybox.min.js";
		js.type = "text/javascript";
		docHead.appendChild(js);
	}

	
	if(numCharts > 0) {
		var js = document.createElement("script");
		js.src = "https://www.gstatic.com/charts/loader.js";
		js.type = "text/javascript";
		js.onload = function() {
			google.charts.load('current', {'packages':['corechart']});

			for(var i in drawChartCallbacks) {
				google.charts.setOnLoadCallback(window[drawChartCallbacks[i]]);
			}
		};
		docHead.appendChild(js);
	}

})(jQuery);
</script>

>>
