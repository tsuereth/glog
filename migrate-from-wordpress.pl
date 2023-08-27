#!/usr/bin/env perl
use FindBin;
use lib $FindBin::Bin;
use strict;
use warnings;
use File::Basename;
use File::Path;
use Time::Local;
use XML::Simple;
$XML::Simple::PREFERRED_PARSER = 'XML::Parser';

sub usage() {
	print "Usage: $0 WP_EXPORT.xml POST/DESTINATION/DIRECTORY\n";
	exit(0);
}

if(scalar(@ARGV) < 2) {
	usage();
}
my $wp_export = $ARGV[0];
if(! -f($wp_export)) {
	print "Couldn't find WP EXPORT file \"$wp_export\"\n";
	usage();
}
my $destination = $ARGV[1];
if(! -d($destination)) {
	print "Destination doesn't exist: \"$destination\"\n";
	usage();
}

my $wp_export_xml = XMLin($wp_export, ForceArray => 1, KeyAttr => []);

foreach my $item(@{$wp_export_xml->{'channel'}[0]->{'item'}}) {
	my @post_categories = ();
	my @post_games = ();
	my @post_platforms = ();
	my $post_rating = undef;
	foreach my $category(@{$item->{'category'}}) {
		my $domain = $category->{'domain'};
		my $content = $category->{'content'};
		if($domain eq 'category') {
			push(@post_categories, $content);
		}
		elsif($domain eq 'game') {
			push(@post_games, $content);
		}
		elsif($domain eq 'platform') {
			push(@post_platforms, $content);
		}
		elsif($domain eq 'rating') {
			$post_rating = $content;
		}
		else {
			die "UNKNOWN ITEM CATEGORY: $domain\n";
		}
	}

	$item->{'wp:post_date'}[0] =~ /^(\d{4})-(\d{2})-(\d{2}) (\d{2}):(\d{2}):(\d{2})$/;
	my ($year, $month, $day, $hour, $minute, $second) = ($1, $2, $3, $4, $5, $6);
	my $post_timestamp_local = timegm($6, $5, $4, $3, $2 - 1, $1);
	$item->{'wp:post_date_gmt'}[0] =~ /^(\d{4})-(\d{2})-(\d{2}) (\d{2}):(\d{2}):(\d{2})$/;
	my $post_timestamp_utc = timegm($6, $5, $4, $3, $2 - 1, $1);
	my $utc_offset_seconds = $post_timestamp_local - $post_timestamp_utc;
	my $utc_offset_hours = abs(int($utc_offset_seconds / 3600));
	my $utc_offset_minutes = abs(int($utc_offset_seconds / 60) % 60);
	my $utc_offset_direction = '-';
	if($utc_offset_seconds > 0) { $utc_offset_direction = '+'; }
	my $post_time = sprintf(
		"%04d-%02d-%02dT%02d:%02d:%02d%s%02d:%02d",
		$year, $month, $day, $hour, $minute, $second,
		$utc_offset_direction,
		$utc_offset_hours,
		$utc_offset_minutes
	);

	my $post_text = $item->{'content:encoded'}[0];
	my $post_title = $item->{'title'}[0];
	my $post_id = $item->{'wp:post_id'}[0];
	my $post_name = $item->{'wp:post_name'}[0];

	$post_title =~ s/"/\\"/g;

	my $post_content = "+++\n";
	$post_content .= "date = \"$post_time\"\n";
	$post_content .= "title = \"$post_title\"\n";
	$post_content .= "slug = \"$post_name\"\n";
	if(scalar(@post_categories) > 0) {
		$post_content .= "category = [\"" . join('"], ["', @post_categories) . "\"]\n";
	}
	if(scalar(@post_games) > 0) {
		$post_content .= "game = [\"" . join('"], ["', @post_games) . "\"]\n";
	}
	if(scalar(@post_platforms) > 0) {
		$post_content .= "platform = [\"" . join('"], ["', @post_platforms) . "\"]\n";
	}
	if(defined($post_rating)) {
		$post_content .= "rating = [\"" . $post_rating . "\"]\n";
	}
	$post_content .= "+++\n\n";

	# Fix absolute links.
	$post_text =~ s#https://tsuereth\.com##g;
	if($post_text =~ /tsuereth\.com/) {
		die "Bad link! in $post_id\n";
	}
	# Replace [game] shortcodes.
	$post_text =~ s/\[game\]([^\[]+)\[\/game\]/\{\{% game "$1" %\}\}$1\{\{% \/game %\}\}/g;
	$post_text =~ s/\[game name="([^"]+)"\]([^\[]+)\[\/game\]/\{\{% game "$1" %\}\}$2\{\{% \/game %\}\}/g;
	# Replace [platform] shortcodes.
	$post_text =~ s/\[platform\]([^\[]+)\[\/platform\]/\{\{% platform "$1" %\}\}$1\{\{% \/platform %\}\}/g;
	$post_text =~ s/\[platform name="([^"]+)"\]([^\[]+)\[\/platform\]/\{\{% platform "$1" %\}\}$2\{\{% \/platform %\}\}/g;
	# Append '  ' to single line breaks (for markdown).
	$post_text =~ s/(\S+)\n(\S+)/$1  \n$2/gm;
	$post_content .= $post_text;

	my $filename = $destination . '/migrated-from-wordpress/' . $year . '/' . $month . '/' . $post_id . '.md';
	File::Path::make_path(dirname($filename));
	print "Writing $filename\n";

	open(my $post, '>', $filename);
	print $post $post_content;
	close($post);
}

print "Complete.\n";
