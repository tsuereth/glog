#!/usr/bin/env perl
use FindBin;
use lib $FindBin::Bin;
use strict;
use utf8;
use warnings;

use DateTime;
use DateTime::Format::Strptime;
use Glog;

binmode STDOUT, ":utf8";
my $datetime_fmt = DateTime::Format::Strptime->new(
	"pattern" => "%Y-%m-%dT%H:%M:%S",
	"strict" => 1,
	"time_zone" => "local",
	"on_error" => "croak"
);

my $post_start_datetime = undef;
if(scalar(@ARGV) >= 1) {
	$post_start_datetime = $datetime_fmt->parse_datetime($ARGV[0]);
}

my $post_end_datetime = undef;
if(scalar(@ARGV) >= 2) {
	$post_end_datetime = $datetime_fmt->parse_datetime($ARGV[1]);
}

# Collect metadata from all posts.
my @metadata;
Glog::GetAllPostMetadata(\@metadata);

# Filter out posts that aren't within the given start and end times.
if(defined($post_start_datetime) || defined($post_end_datetime)) {
	my @metadata_filtered;
	foreach my $post(@metadata) {
		if(!defined($post->{"date"})) {
			next;
		}
		my $post_datetime = $datetime_fmt->parse_datetime($post->{"date"});

		my($include_start, $include_end) = (1, 1);
		if(defined($post_start_datetime)) {
			$include_start = (DateTime->compare($post_datetime, $post_start_datetime) >= 0);
		}
		if(defined($post_end_datetime)) {
			$include_end = (DateTime->compare($post_datetime, $post_end_datetime) <= 0);
		}

		if($include_start && $include_end) {
			push(@metadata_filtered, $post);
		}
	}

	@metadata = @metadata_filtered;
}

# For each found game, count the game's associated tags.
my %tags;
my %foundgames;
foreach my $post(@metadata) {
	if(!defined($post->{"category"}) || !grep(/^Playing A Game$/, @{$post->{"category"}})) {
		next;
	}

	if(defined($post->{"game"})) {
		foreach my $game(@{$post->{"game"}}) {
			if(defined($foundgames{$game})) {
				next;
			}

			$foundgames{$game} = 1;

			# TODO?: Use IGDB Cache directly, rather than stumbling through flat files.
			my %gamedata;
			Glog::GetGameMetadata($game, \%gamedata);
			if(defined($gamedata{"tag"})) {
				foreach my $tag(@{$gamedata{"tag"}}) {
					if(!defined($tags{$tag})) {
						$tags{$tag} = 1;
					}
					else {
						$tags{$tag}++;
					}
				}
			}
			else {
				warn "No tag(s) found for game \"" . $game . "\"";
			}
		}
	}
}

# Sort the tags by their occurrence count (largest first).
my @keys_sorted = sort {$tags{$b} <=> $tags{$a}} keys %tags;

# Print the tag counts from largest to smallest.
print "Tag,Count\n";
foreach my $key(@keys_sorted) {
	my $count = $tags{$key};

	printf("%s,%d\n", $key, $count);
}
