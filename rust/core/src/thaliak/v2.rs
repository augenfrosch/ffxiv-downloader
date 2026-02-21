use std::fmt::Display;

use anyhow::Result;
use futures::TryFutureExt;
use reqwest::Client;
use serde::Deserialize;

#[derive(Debug, Deserialize)]
pub struct Patch {
	pub version_string: String,
	pub remote_url: String,
	pub size: u64,
	// ...
}

// #[derive(Debug, Deserialize)]
// pub struct RepositoryPatchesResponse {
// 	patches: Vec<Patch>,
// 	// ...
// }

#[derive(Debug, Deserialize)]
pub struct LatestPatch {
	pub version_string: String,
	// ...
}

#[derive(Debug, Deserialize)]
pub struct RepositoryResponse {
	pub name: String,
    pub description: String,
    pub latest_patch: LatestPatch,
}

const BASE_URL: &str = "https://thaliak.xiv.dev/api/v2beta";

pub async fn get_repository_metadata(
    client: &Client,
    slug: impl Display,
) -> Result<RepositoryResponse> {
	client
		.get(format!("{BASE_URL}/repositories/{slug}"))
		.send()
        .await?
        .json()
		.err_into()
        .await
}

pub async fn get_patch_metadata(
	client: &Client,
    slug: impl Display,
	patch_name: impl Display,
) -> Result<Patch> {
	client
		.get(format!("{BASE_URL}/repositories/{slug}/patches/{patch_name}"))
		.send()
        .await?
        .json()
		.err_into()
        .await
}
