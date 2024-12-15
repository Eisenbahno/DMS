import requests
import json
import os

API_URL = "http://localhost:8080/document"
TEST_DOCUMENT_NAME = "TestDocument"
TEST_FILE_PATH = r"/Users/stefan/Desktop/DMS/uploads/Bachelor-Proposal_Rezk.pdf"  # Use raw string or escape backslashes


def fetch():
    # Fetch all documents
    print("Fetching all documents...")
    response = requests.get(API_URL)
    response.raise_for_status()  # Raise an exception for bad status codes (4xx or 5xx)
    response = response.json()
    print(f"[SUCCESS] Documents fetched. There are {len(response)} documents!")
    if len(response) == 0:
        return 0
    return response[len(response) - 1].get("id")  # return latest id


def add():
    # Add a new document
    print("Adding a new document...")
    new_document = {"name": TEST_DOCUMENT_NAME}
    response = requests.post(API_URL, headers={"Content-Type": "application/json"}, json=new_document)
    response.raise_for_status()
    created_document = response.json()
    print("[SUCCESS] Document added!")


def delete(docID):
    # Delete the created document
    print(f"Deleting the document with id {docID}...")
    response = requests.delete(f"{API_URL}/{docID}")
    response.raise_for_status()
    print("[SUCCESS] Document deleted.")


def upload(docID):
    # Upload a file
    print("Uploading file...")
    if not os.path.exists(TEST_FILE_PATH):
        raise FileNotFoundError(f"File not found: {TEST_FILE_PATH}")
    with open(TEST_FILE_PATH, "rb") as f:  # open file in binary read mode
        files = {"document": (os.path.basename(TEST_FILE_PATH), f)}
        response = requests.put(f"{API_URL}/{docID}/upload", files=files)
    response.raise_for_status()
    print("[SUCCESS] File uploaded successfully.")


try:
    fetch()

    add()

    docID = fetch()

    delete(docID)

    fetch()

    # Re-add the document for upload testing
    print("Re-adding the document for upload testing...")
    add()
    docID = fetch()

    upload(docID)

    print("All tests completed successfully.")

except requests.exceptions.RequestException as e:
    print(f"[FAILED] Request error: {e}")
    if response is not None:  # print the response text if available for better debugging
        print(f"Response text: {response.text}")
    exit(1)
except (ValueError, FileNotFoundError) as e:
    print(f"[FAILED] {e}")
    exit(1)
except json.JSONDecodeError as e:
    print(f"[FAILED] Could not decode JSON response: {e}. Response text: {response.text}")
    exit(1)
except Exception as e:
    print(f"[FAILED] An unexpected error occurred: {e}")
    exit(1)
