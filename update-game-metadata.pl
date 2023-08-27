#!/usr/bin/env perl
use FindBin;
use lib $FindBin::Bin;
use strict;
use utf8;
use warnings;

use GameCache;
use Glog;
use IGDB;

binmode STDOUT, ":utf8";

if(scalar(@ARGV) < 2) {
	die "Usage: ./update-game-metadata.pl LOCAL_CACHE_FILE IGDB_CLIENT_ID IGDB_CLIENT_SECRET [forceupdate]";
}

my $cache_file = $ARGV[0];
my $igdb_client_id = $ARGV[1];
my $igdb_client_secret = $ARGV[2];

my $forceupdate = 0;
if(scalar(@ARGV) >= 4) {
	if($ARGV[3] =~ /^forceupdate$/i) {
		$forceupdate = 1;
	}
}

# Find all of the games referenced in all Glog content.
my @gamenames;
Glog::GetAllGameNames(\@gamenames);

# Create or load the local game cache.
my $cacheinitialized = 0;
if(-f($cache_file)) {
	print "Opening local cache at \"$cache_file\" ...\n";
	$cacheinitialized = GameCache::LoadFromFlatFile($cache_file);
}

if(!$cacheinitialized) {
	print "Creating new cache to save at \"$cache_file\" ...\n";
	$cacheinitialized = GameCache::Initialize();
}

# Log in to the IGDB API.
IGDB::Login($igdb_client_id, $igdb_client_secret);

# Get the IGDB IDs for each game.
# TODO: Implement IGDB::GetIDs(...?) to batch-request IDs for title strings!
my %games;
foreach my $gamename(@gamenames) {
	my $igdb_id = Glog::GetIgdbId($gamename);
	if(defined($igdb_id)) {
		$games{$igdb_id} = {
			"id" => $igdb_id,
			"local_name" => $gamename
		};
	}
	else {
		print "No IGDB ID for game \"$gamename\"\n";
	}
}

my (@collection_ids, @franchise_ids, @game_mode_ids, @genre_ids, @involved_company_ids, @player_perspective_ids, @theme_ids);

sub CollectGameData {
	my ($game_hashref) = @_;

	my $id = $game_hashref->{"id"};

	# See if a game has been updated since the last cache update.
	if($forceupdate == 0) {
		my $cache_lastupdated = GameCache::GetGameLastUpdated($id);
		if($game_hashref->{"updated_at"} <= $cache_lastupdated) {
			# If the cache is at least as recent, skip updating it.
			return;
		}
	}

	$games{$id}->{"_updated"} = 1;

	# 1) Store metadata IDs for later reference (when updating the local cache);
	# 2) Save off the game's sanitized values (also for updating the local cache).

	$games{$id}->{"category"} = $game_hashref->{"category"};

	my $game_collection = -1;
	if(defined($game_hashref->{"collection"})) {
		$game_collection = $game_hashref->{"collection"};
		push(@collection_ids, $game_hashref->{"collection"});
	}
	$games{$id}->{"collection"} = $game_collection;

	my $game_franchise = -1;
	my $game_franchises = "";
	if(defined($game_hashref->{"franchise"})) {
		$game_franchise = $game_hashref->{"franchise"};
		push(@franchise_ids, $game_hashref->{"franchise"});
	}
	# A better title for this IGDB field would have been "additional franchises."
	if(defined($game_hashref->{"franchises"})) {
		$game_franchises = join(",", @{$game_hashref->{"franchises"}});
		push(@franchise_ids, @{$game_hashref->{"franchises"}});
	}
	$games{$id}->{"franchise"} = $game_franchise;
	$games{$id}->{"franchises"} = $game_franchises;

	my $game_game_modes = "";
	if(defined($game_hashref->{"game_modes"})) {
		$game_game_modes = join(",", @{$game_hashref->{"game_modes"}});
		push(@game_mode_ids, @{$game_hashref->{"game_modes"}});
	}
	$games{$id}->{"game_modes"} = $game_game_modes;

	my $game_genres = "";
	if(defined($game_hashref->{"genres"})) {
		$game_genres = join(",", @{$game_hashref->{"genres"}});
		push(@genre_ids, @{$game_hashref->{"genres"}});
	}
	$games{$id}->{"genres"} = $game_genres;

	my $game_involved_companies = "";
	if(defined($game_hashref->{"involved_companies"})) {
		$game_involved_companies = join(",", @{$game_hashref->{"involved_companies"}});
		push(@involved_company_ids, @{$game_hashref->{"involved_companies"}});
	}
	$games{$id}->{"involved_companies"} = $game_involved_companies;

	# Log if the given title is different from the Glog metadata.
	my $local_name = Glog::NormalizeGameName($games{$id}->{"local_name"});
	my $igdb_name = Glog::NormalizeGameName($game_hashref->{"name"});
	if($local_name ne $igdb_name) {
		print "IGDB ID $id is locally \"$local_name\" but IGDB calls it \"$igdb_name\"\n";
	}
	$games{$id}->{"name"} = $game_hashref->{"name"};

	my $game_player_perspectives = "";
	if(defined($game_hashref->{"player_perspectives"})) {
		$game_player_perspectives = join(",", @{$game_hashref->{"player_perspectives"}});
		push(@player_perspective_ids, @{$game_hashref->{"player_perspectives"}});
	}
	$games{$id}->{"player_perspectives"} = $game_player_perspectives;

	my $game_themes = "";
	if(defined($game_hashref->{"themes"})) {
		$game_themes = join(",", @{$game_hashref->{"themes"}});
		push(@theme_ids, @{$game_hashref->{"themes"}});
	}
	$games{$id}->{"themes"} = $game_themes;

	$games{$id}->{"updated_at"} = $game_hashref->{"updated_at"};

	my $game_url = "";
	if(defined($game_hashref->{"url"})) {
		$game_url = $game_hashref->{"url"};
	}
	$games{$id}->{"url"} = $game_url;
}

# Update metadata for each game with an IGDB ID.
my @game_ids = keys %games;

my @game_fields = (
	"id",
	"category",
	"collection",
	"franchise",
	"franchises",
	"game_modes",
	"genres",
	"involved_companies",
	"name",
	"player_perspectives",
	"themes",
	"updated_at",
	"url"
);

IGDB::GetGamesCallWithHashRef(\@game_ids, \@game_fields, \&CollectGameData);

# Update the local cache.
my @metadata_fields = (
	"id",
	"name"
);

my %companies;
sub _UpdateInvolvedCompany {
	my ($involved_company_hashref) = @_;

	# Track this company ID.
	$companies{$involved_company_hashref->{"company"}} = 1;

	GameCache::UpdateInvolvedCompany($involved_company_hashref);
}

if(scalar(@collection_ids) > 0) {
	IGDB::GetCollectionsCallWithHashRef(\@collection_ids, \@metadata_fields, \&GameCache::UpdateCollection);
}
if(scalar(@franchise_ids) > 0) {
	IGDB::GetFranchisesCallWithHashRef(\@franchise_ids, \@metadata_fields, \&GameCache::UpdateFranchise);
}
if(scalar(@game_mode_ids) > 0) {
	IGDB::GetGameModesCallWithHashRef(\@game_mode_ids, \@metadata_fields, \&GameCache::UpdateGameMode);
}
if(scalar(@genre_ids) > 0) {
	IGDB::GetGenresCallWithHashRef(\@genre_ids, \@metadata_fields, \&GameCache::UpdateGenre);
}
if(scalar(@involved_company_ids) > 0) {
	# This one has different metadata fields than everything else, because of course it does.
	my @involved_company_fields = (
		"id",
		"company"
	);

	IGDB::GetInvolvedCompaniesCallWithHashRef(\@involved_company_ids, \@involved_company_fields, \&_UpdateInvolvedCompany);

	my @company_ids = keys %companies;
	if(scalar(@company_ids) > 0) {
		IGDB::GetCompaniesCallWithHashRef(\@company_ids, \@metadata_fields, \&GameCache::UpdateCompany);
	}
}
if(scalar(@player_perspective_ids) > 0) {
	IGDB::GetPlayerPerspectivesCallWithHashRef(\@player_perspective_ids, \@metadata_fields, \&GameCache::UpdatePlayerPerspective);
}
if(scalar(@theme_ids) > 0) {
	IGDB::GetThemesCallWithHashRef(\@theme_ids, \@metadata_fields, \&GameCache::UpdateTheme);
}

# Update games last, to ensure that the relevant metadata is available.
foreach my $id(keys %games) {
	if(defined($games{$id}->{"_updated"})) {
		GameCache::UpdateGame($games{$id}, $forceupdate);

		# And we can write the updated metadata out at the same time.
		my $gamename = $games{$id}->{"local_name"};
		my $igdb_url = $games{$id}->{"url"};

		my @tags;
		GameCache::GetGameTags($id, \@tags);

		print "Writing metadata for \"$gamename\"\n";
		Glog::WriteGameMetadata($gamename, $id, $igdb_url, \@tags);
	}
}

# Save out the updated local cache.
GameCache::SaveToFlatFile($cache_file);
