const API_ROUTE = "/api/recipe/";
const LOW_RATING_COLOR = "red";
const HIGH_RATING_COLOR = "green";

$(document).ready(() =>
{
    $(document).on("click", ".vote", vote);

});

async function vote()
{
    const recipeId = $("#recipe").attr("recipe-id");
    const voteType = $(this).attr("vote-type");
    const voteUrl = API_ROUTE + `vote?recipeId=${recipeId}&voteType=${voteType}`;

    const response = await fetch(voteUrl, { method: "PUT" });

    console.log(response.statusText)
    if (!response.ok) return;
    updateVotePercentage();

    // Change vote Icon and API call
    const icon = $(this).attr("id");
    if (voteType === "NoVote")
    {
        $(this).attr("src", `/images/icons/hand-${icon}.svg`);
        $(this).attr("vote-type", $(this).attr("base-vote"));
        return
    }

    $(this).attr("vote-type", "NoVote");
    $(this).attr("src", `/images/icons/hand-${icon}-fill.svg`);

    if (voteType === "UpVote")
    {
        $("#thumbs-down").attr("src", "/images/icons/hand-thumbs-down.svg");
        $("#thumbs-down").attr("vote-type", "DownVote");
    }
    else
    {
        $("#thumbs-up").attr("src", "/images/icons/hand-thumbs-up.svg");
        $("#thumbs-up").attr("vote-type", "UpVote");
    }
}

async function updateVotePercentage()
{
    const recipeId = $("#recipe").attr("recipe-id");
    const ratingUrl = API_ROUTE + `rating?recipeId=${recipeId}`;

    const response = await fetch(ratingUrl);
    if (!response.ok) return;

    let rating = await response.json();
    rating = (rating * 100).toFixed(0) + "%";
    $("#votePercent").attr("style", `color: color-mix(in oklch, ${LOW_RATING_COLOR}, ${HIGH_RATING_COLOR} ${rating});`)
    $("#votePercent").text(rating)
}