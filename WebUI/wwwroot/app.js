const apiUrl = 'http://localhost:8080/document';

// Function to fetch and display Todo items
function fetchDocuments() {
    console.log('Fetching Documents...');
    fetch(apiUrl)
        .then(response => response.json())
        .then(data => {
            const documentList = document.getElementById('documentList');
            documentList.innerHTML = ''; // Clear the list before appending new items
            data.forEach(document => {
                // Create list item with delete and toggle complete buttons
                const li = document.createElement('li');
                li.innerHTML = `
                    <span>Document: ${document.name} | Id: ${document.id}</span>
                    <button class="delete" style="margin-left: 10px;" onclick="deleteDocument(${document.id})">Delete</button>
                    </button>
                `;
                documentList.appendChild(li);
            });
        })
        .catch(error => console.error('Fehler beim Abrufen der Document-Items:', error));
}


// Function to add a new tasdocumentk
function addDocument() {
    const documentName = document.getElementById('documentName').value;
    const errorDiv = document.getElementById('errorMessages'); //Div für Fehlermeldungen


    const newDocument = {
        name: documentName
    };

    fetch(apiUrl, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(newDocument)
    })
        .then(response => {
            if (response.ok) {
                fetchDocuments(); // Refresh the list after adding
                document.getElementById('documentName').value = ''; // Clear the input field
            } else {
                // Neues Handling für den Fall eines Fehlers (z.B. leeres Namensfeld)
                response.json().then(err => {
                    errorDiv.innerHTML = `<ul>` + Object.values(err.errors).map(e => `<li>${e}</li>`).join('') + `</ul>`;
                });
            }
        })
        .catch (error => console.error('Fehler:', error));
}


// Function to delete a document
function deleteDocument(id) {
    fetch(`${apiUrl}/${id}`, {
        method: 'DELETE'
    })
        .then(response => {
            if (response.ok) {
                fetchDocuments(); // Refresh the list after deletion
            } else {
                console.error('Fehler beim Löschen der Aufgabe.');
            }
        })
        .catch(error => console.error('Fehler:', error));
}


// Load todo items on page load
document.addEventListener('DOMContentLoaded', fetchDocuments);
