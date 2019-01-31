// Gets data for modal / opens modal
function pushDataToModal(albumId)
{
    $.ajax({
        contentType: 'application/json',
        type: "GET",
        url: "https://jsonplaceholder.typicode.com/photos?albumId=" + albumId,
        success: function (data, textStatus, jqXHR) {
            var count = 0;
            data.forEach(function (photo) {
                count++;
                $("#modal-target").append(
                    "<div class=\"album-photo-container clearfix\"> " +
                    "<img class=\"img-thumbnail\" src=\"" + photo.url + "\" />" +
                    "<p>" + count + ".) " + photo.title + "</p>" + 
                    "</div>"
                );
            });
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.log(jqXHR.statusText);
        }
    });
}

// Closes and removes data from modal
$('.dismiss-modal-data').click(function (e) {
    var modalContent = document.getElementById("modal-target");
    var cNode = modalContent.cloneNode(false);
    modalContent.parentNode.replaceChild(cNode, modalContent);
});