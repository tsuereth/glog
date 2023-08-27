package IGDB;

use strict;
use utf8;
use warnings;

use HTTP::Request;
use JSON;
use LWP::UserAgent;

my $API_BASE = "https://api.igdb.com/v4/";

my $REQUEST_ITEMS_MAX = 500; # Can only request N items at a time.

# Source: https://api-docs.igdb.com/#game-enums
my %CATEGORIES = (
	0 => "Main game",
	1 => "DLC / Addon",
	2 => "Expansion",
	3 => "Bundle",
	4 => "Standalone expansion",
	5 => "Mod",
	6 => "Episode",
	7 => "Season",
	8 => "Remake",
	9 => "Remaster",
	10 => "Expanded Game",
	11 => "Port",
	12 => "Fork"
);

sub GetCategories {
	return %CATEGORIES;
}

my $_ua = LWP::UserAgent->new(
	"agent" => "IGDB/LWP"
);

my ($_client_id, $_access_token) = (undef, undef);

sub Login {
	if(scalar(@_) < 2) {
		die "Insuffcient arguments to Login(client_id, client_secret)";
	}

	my ($client_id, $client_secret) = @_;

	my $url = "https://id.twitch.tv/oauth2/token?client_id=" . $client_id . "&client_secret=" . $client_secret . "&grant_type=client_credentials";

	my $request = HTTP::Request->new(POST => $url);
	my $response = $_ua->request($request);
	if(!$response->is_success) {
		#die $request->as_string();
		die "Error generating access token: " . $response->status_line;
	}
	my $response_decoded = decode_json($response->decoded_content);

	$_client_id = $client_id;
	$_access_token = $response_decoded->{"access_token"};
}

sub GetGamesCallWithHashRef {
	if (!defined($_client_id) || !defined($_access_token)) {
		die "Must Login before calling GetGamesCallWithHashRef!";
	}

	if(scalar(@_) < 3) {
		die "Insufficient arguments to GetGamesCallWithHashRef(ids_arrayref, fields_arrayref, game_hashref_callback)";
	}

	my ($ids_arrayref, $fields_arrayref, $game_hashref_callback) = @_;

	my $ids_index = 0;
	while($ids_index < scalar(@{$ids_arrayref})) {
		my $url = $API_BASE . "games";

		my @request_ids;
		while(
			($ids_index < scalar(@{$ids_arrayref})) and
			(scalar(@request_ids) < $REQUEST_ITEMS_MAX)
		) {
			push(@request_ids, $ids_arrayref->[$ids_index]);
			$ids_index++;
		}

		my $request_count = scalar(@request_ids);

		# Request games from the IGDB API.
		my $request = HTTP::Request->new(POST => $url);
		$request->header("Accept" => "application/json");
		$request->header("Client-ID" => $_client_id);
		$request->header("Authorization" => "Bearer " . $_access_token);
		$request->content("fields " . join(",", @{$fields_arrayref}) . ";where id = (" . join(",", @request_ids) . ");limit $REQUEST_ITEMS_MAX;");
		my $response = $_ua->request($request);
		if(!$response->is_success) {
			#die $request->as_string();
			die "Error requesting games from IGDB [$url]: " . $response->status_line;
		}
		my $response_decoded = decode_json($response->decoded_content);

		my %missing_ids = map { $_ => 1 } @request_ids;
		foreach my $game(@{$response_decoded}) {
			my $game_id = $game->{"id"};
			delete $missing_ids{$game_id};

			&$game_hashref_callback($game);
		}

		my $missing_count = scalar(keys %missing_ids);
		if($missing_count > 0) {
			warn "games response was missing requested IDs: [" . join(",", keys %missing_ids) . "]";
		}
	}
}

sub _GetMetadataCallWithHashRef {
	if (!defined($_client_id) || !defined($_access_token)) {
		die "Must Login before calling GetGamesCallWithHashRef!";
	}

	if(scalar(@_) < 4) {
		die "Insufficient arguments to _GetMetadataCallWithHashRef(metadata_typename, ids_arrayref, fields_arrayref, metadata_hashref_callback)";
	}

	my ($metadata_typename, $ids_arrayref, $fields_arrayref, $metadata_hashref_callback) = @_;

	my $ids_index = 0;
	while($ids_index < scalar(@{$ids_arrayref})) {
		my $url = $API_BASE . $metadata_typename;

		my @request_ids;
		while(
			($ids_index < scalar(@{$ids_arrayref})) and
			(scalar(@request_ids) < $REQUEST_ITEMS_MAX)
		) {
			push(@request_ids, $ids_arrayref->[$ids_index]);
			$ids_index++;
		}

		my $request_count = scalar(@request_ids);

		# Request the type from the IGDB API.
		my $request = HTTP::Request->new(POST => $url);
		$request->header("Accept" => "application/json");
		$request->header("Client-ID" => $_client_id);
		$request->header("Authorization" => "Bearer " . $_access_token);
		$request->content("fields " . join(",", @{$fields_arrayref}) . ";where id = (" . join(",", @request_ids) . ");limit $REQUEST_ITEMS_MAX;");
		my $response = $_ua->request($request);
		if(!$response->is_success) {
			die "Error requesting type \"$metadata_typename\" from IGDB [$url]: " . $response->status_line;
		}
		my $response_decoded = decode_json($response->decoded_content);

		my %missing_ids = map { $_ => 1 } @request_ids;
		foreach my $item(@{$response_decoded}) {
			my $item_id = $item->{"id"};
			delete $missing_ids{$item_id};

			&$metadata_hashref_callback($item);
		}

		my $missing_count = scalar(keys %missing_ids);
		if($missing_count > 0) {
			warn "$metadata_typename response was missing requested IDs: [" . join(",", keys %missing_ids) . "]";
		}
	}
}

sub GetCollectionsCallWithHashRef {
	if(scalar(@_) < 3) {
		die "Insufficient arguments to GetCollectionsCallWithHashRef(ids_arrayref, fields_arrayref, collection_hashref_callback)";
	}

	my ($ids_arrayref, $fields_arrayref, $collection_hashref_callback) = @_;
	_GetMetadataCallWithHashRef("collections", $ids_arrayref, $fields_arrayref, $collection_hashref_callback);
}

sub GetCompaniesCallWithHashRef {
	if(scalar(@_) < 3) {
		die "Insufficient arguments to GetCompaniesCallWithHashRef(ids_arrayref, fields_arrayref, company_hashref_callback)";
	}

	my ($ids_arrayref, $fields_arrayref, $company_hashref_callback) = @_;
	_GetMetadataCallWithHashRef("companies", $ids_arrayref, $fields_arrayref, $company_hashref_callback);
}

sub GetFranchisesCallWithHashRef {
	if(scalar(@_) < 3) {
		die "Insufficient arguments to GetFranchisesCallWithHashRef(ids_arrayref, fields_arrayref, franchise_hashref_callback)";
	}

	my ($ids_arrayref, $fields_arrayref, $franchise_hashref_callback) = @_;
	_GetMetadataCallWithHashRef("franchises", $ids_arrayref, $fields_arrayref, $franchise_hashref_callback);
}

sub GetGameModesCallWithHashRef {
	if(scalar(@_) < 3) {
		die "Insufficient arguments to GetGameModesCallWithHashRef(ids_arrayref, fields_arrayref, gamemode_hashref_callback)";
	}

	my ($ids_arrayref, $fields_arrayref, $gamemode_hashref_callback) = @_;
	_GetMetadataCallWithHashRef("game_modes", $ids_arrayref, $fields_arrayref, $gamemode_hashref_callback);
}

sub GetGenresCallWithHashRef {
	if(scalar(@_) < 3) {
		die "Insufficient arguments to GetGenresCallWithHashRef(ids_arrayref, fields_arrayref, genre_hashref_callback)";
	}

	my ($ids_arrayref, $fields_arrayref, $genre_hashref_callback) = @_;
	_GetMetadataCallWithHashRef("genres", $ids_arrayref, $fields_arrayref, $genre_hashref_callback);
}

sub GetInvolvedCompaniesCallWithHashRef {
	if(scalar(@_) < 3) {
		die "Insufficient arguments to GetInvolvedCompaniesCallWithHashRef(ids_arrayref, fields_arrayref, involved_company_hashref_callback)";
	}

	my ($ids_arrayref, $fields_arrayref, $involved_company_hashref_callback) = @_;
	_GetMetadataCallWithHashRef("involved_companies", $ids_arrayref, $fields_arrayref, $involved_company_hashref_callback);
}

sub GetPlayerPerspectivesCallWithHashRef {
	if(scalar(@_) < 3) {
		die "Insufficient arguments to GetPlayerPerspectivesCallWithHashRef(ids_arrayref, fields_arrayref, playerperspective_hashref_callback)";
	}

	my ($ids_arrayref, $fields_arrayref, $playerperspective_hashref_callback) = @_;
	_GetMetadataCallWithHashRef("player_perspectives", $ids_arrayref, $fields_arrayref, $playerperspective_hashref_callback);
}

sub GetThemesCallWithHashRef {
	if(scalar(@_) < 3) {
		die "Insufficient arguments to GetThemesCallWithHashRef(ids_arrayref, fields_arrayref, theme_hashref_callback)";
	}

	my ($ids_arrayref, $fields_arrayref, $theme_hashref_callback) = @_;
	_GetMetadataCallWithHashRef("themes", $ids_arrayref, $fields_arrayref, $theme_hashref_callback);
}

1;
