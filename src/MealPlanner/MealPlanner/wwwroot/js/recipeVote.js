const API_ROUTE = "/api/recipe/";

$(document).ready(() => {
    $(document).on("click", ".vote", vote);
});

async function vote() {
    const recipeId = $("#recipe").attr("recipe-id");
    const voteType = $(this).attr("vote-type");
    const voteUrl  = API_ROUTE + `vote?recipeId=${recipeId}&voteType=${voteType}`;

    const response = await fetch(voteUrl, { method: "PUT" });
    if (!response.ok) return;
    updateVotePercentage();

    const btnId = $(this).attr("id");

    if (voteType === "NoVote") {
        $(this).removeClass("sr-active");
        $(this).attr("vote-type", $(this).attr("base-vote"));
        return;
    }

    $(this).attr("vote-type", "NoVote").addClass("sr-active");

    if (btnId === "thumbs-up") {
        $("#thumbs-down").removeClass("sr-active").attr("vote-type", "DownVote");
    } else {
        $("#thumbs-up").removeClass("sr-active").attr("vote-type", "UpVote");
    }
}

async function updateVotePercentage() {
    const recipeId  = $("#recipe").attr("recipe-id");
    const ratingUrl = API_ROUTE + `rating?recipeId=${recipeId}`;

    const response = await fetch(ratingUrl);
    if (!response.ok) return;

    const rating = await response.json();
    $("#votePercent").text((rating * 100).toFixed(0) + "%");
}
