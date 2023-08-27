#!/usr/bin/env perl
use FindBin;
use lib $FindBin::Bin;
use strict;
use utf8;
use warnings;

use DateTime;
use DateTime::Format::Strptime;
use Glog;
use IGDB;

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

# Aggregate post stats by game title + platform.
my %games;
foreach my $post(@metadata) {
	if(defined($post->{"game"}) && defined($post->{"platform"})) {
		foreach my $game(@{$post->{"game"}}) {
			# TODO: Fix assumption of one platform per post.
			my $platform = @{$post->{"platform"}}[0];
			# TODO?: Fix assumption of one rating per post.
			my $rating = @{$post->{"rating"}}[0];

			my $key = $game . "::::" . $platform;

			my $gamestats = $games{$key};
			if(!defined($games{$key})) {
				# TODO?: Use IGDB Cache directly, rather than stumbling through flat files.
				my %gamedata;
				Glog::GetGameMetadata($game, \%gamedata);

				my $gametype = undef;
				if(defined($gamedata{"tag"})) {
					my %gametags = map {$_ => 1} @{$gamedata{"tag"}};

					my %categories = IGDB::GetCategories();
					foreach my $category(values %categories) {
						if(defined($gametags{$category})) {
							$gametype = $category;
							last;
						}
					}

					if(!defined($gametype)) {
						warn "Failed to detect game type for game \"" . $game . "\"";
					}
				}
				else {
					warn "No tag(s) found for game \"" . $game . "\"";
				}

				$games{$key} = {
					"game" => $game,
					"platform" => $platform,
					"type" => $gametype,
					"date_first" => $post->{"date"},
					"date_last" => $post->{"date"},
					"rating" => $rating,
					"numposts" => 1
				};
			}
			else {
				$games{$key}->{"numposts"} += 1;

				my $post_datetime = $datetime_fmt->parse_datetime($post->{"date"});

				my $game_first_datetime = $datetime_fmt->parse_datetime($games{$key}->{"date_first"});
				if(DateTime->compare($post_datetime, $game_first_datetime) < 0) {
					$games{$key}->{"date_first"} = $post->{"date"};
				}

				my $game_last_datetime = $datetime_fmt->parse_datetime($games{$key}->{"date_last"});
				if(DateTime->compare($post_datetime, $game_last_datetime) > 0) {
					$games{$key}->{"date_last"} = $post->{"date"};
					if(defined($post->{"rating"})) {
						# TODO?: Fix assumption of one rating per post.
						$games{$key}->{"rating"} = $post->{"rating"}[0];
					}
				}
			}
		}
	}
}

sub game_sorter {
	my($a, $b) = @_;

	my $firstpost_compare = DateTime->compare(
		$datetime_fmt->parse_datetime($a->{"date_first"}),
		$datetime_fmt->parse_datetime($b->{"date_first"})
	);
	if($firstpost_compare != 0) {
		return $firstpost_compare;
	}

	my $title_compare = $a->{"game"} cmp $b->{"game"};
	return $title_compare;
}

# Sort the game keys by when they were first posted.
my @keys_sorted = sort { game_sorter($games{$a}, $games{$b}) } keys %games;

# Print the results in first-posted order.
print "Title,Platform,Type,Firstposted,Lastposted,Rating,Numposts\n";
foreach my $key(@keys_sorted) {
	my $game = $games{$key};

	printf("\"%s\",%s,%s,%s,%s,%s,%d\n",
		defined($game->{"game"}) ? $game->{"game"} : "N/A",
		defined($game->{"platform"}) ? $game->{"platform"} : "N/A",
		defined($game->{"type"}) ? $game->{"type"} : "N/A",
		defined($game->{"date_first"}) ? $game->{"date_first"} : "N/A",
		defined($game->{"date_last"}) ? $game->{"date_last"} : "N/A",
		defined($game->{"rating"}) ? $game->{"rating"} : "N/A",
		defined($game->{"numposts"}) ? $game->{"numposts"} : "N/A"
	);
}
