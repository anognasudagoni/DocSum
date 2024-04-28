import React, { useState, useEffect } from 'react';
import axios from 'axios';
import { useAuth0 } from '@auth0/auth0-react';
import SignIn from '../subcomponents/Signin';

// Subheadings component
const Subheadings = ({ selectedFile }) => {
  const [subtopics, setSubtopics] = useState([]);

  useEffect(() => {
    const fetchData = async () => {
      try {
        if (!selectedFile) return;

        const formData = new FormData();
        formData.append('file', selectedFile);

        const response = await axios.post('http://localhost:5298/DocSum/ExtractSubtopics', formData, {
          headers: {
            'Content-Type': 'multipart/form-data',
            'Accept': '*/*'
          }
        });

        setSubtopics(response.data.subtopics);
      } catch (error) {
        console.error('Error fetching subtopics:', error);
      }
    };

    fetchData();
  }, [selectedFile]);

  return (
    <ul>
      {subtopics.map((subtopic, index) => (
        <li key={index}>{subtopic}</li>
      ))}
    </ul>
  );
};

function Home() {
  const { isAuthenticated, user } = useAuth0();
  const [selectedFile, setSelectedFile] = useState(null);
  const [uploadStatus, setUploadStatus] = useState('');
  const [summaries, setSummaries] = useState('');
  const [filename, setFilename] = useState('Choose a file');
  const [loading, setLoading] = useState(false);
  const [data, setData] = useState([]);

  // Function to handle file selection
  const handleFileChange = (event) => {
    const file = event.target.files[0];
    setSelectedFile(file);
    if (file) {
      setFilename(file.name);
    } else {
      setFilename('Choose a file');
    }
  };

  // Function to handle file upload and summarize
  const handleFileUpload = async () => {
    try {
      // Check if a file is selected
      if (!selectedFile) {
        setUploadStatus('Please select a file');
        return;
      }

      // Start loading animation
      setLoading(true);

      // Create FormData object and append the selected file
      const formData = new FormData();
      formData.append('file', selectedFile);

      // Make a POST request to the server
      const response = await axios.post('http://localhost:5298/DocSum/uploadpdf', formData, {
        headers: {
          'Content-Type': 'multipart/form-data'
        }
      });

      // Update upload status based on the response
      setUploadStatus('File uploaded successfully');

      // Set the summaries received from the server to the state
      setSummaries(response.data.summaries);
    } catch (error) {
      // Handle error if upload fails
      console.error('Error uploading file:', error);
      setUploadStatus('Failed to upload file');
    } finally {
      // Stop loading animation
      setLoading(false);
    }
  };

  useEffect(() => {
    axios.post('http://localhost:5298/DocSum/GetConverstionAll', {}, {
      headers: {
        'Accept': 'text/plain'
      }
    })
      .then(response => {
        setData(response.data);
      })
      .catch(error => {
        console.error('Error fetching data:', error);
      });
  }, []);

  const fetchSummary = async (id, docUrl) => {
    try {
      // Make a POST request to fetch the summary
      const response = await axios.post(`http://localhost:5298/DocSum/GetConverstion?id=${id}`, {}, {
        headers: {
          'accept': 'text/plain'
        }
      });
    
      // Set the summaries received from the server to the state
      setSummaries(response.data.summaries);
      
      // Set the filename to the docUrl
      setFilename(docUrl.substring(9, docUrl.length - 4));
    } catch (error) {
      console.error('Error fetching summary:', error);
      // Handle error if fetching summary fails
    }
  };
  

  return (
    <div className="h-screen">
      <header className="flex h-16 w-full shrink-0 items-center justify-between px-4 md:px-6 bg-gray-100 dark:bg-gray-900 mb-4">
        <h1 className="text-lg font-semibold mx-auto">Text Summarizer</h1>
        
      </header>
      <div className='flex'>
      <div className='bg-gray-100 dark:bg-gray-900 w-[300px] h-screen overflow-y-auto'>
        <h4 className='text-center py-4 '>History</h4>
      {data.slice().reverse().map(item => (
  <button className='p-4 block' key={item.id} onClick={() => fetchSummary(item.id, item.docUrl)}>
    {item.docUrl.substring(9, item.docUrl.length - 4)}
  </button>
))}

      </div>
        <div className="grid w-full max-w-100 gap-6 px-4 text-center">
          <div className="grid grid-cols-2 gap-6">
            {/* Input section */}
            <div className="rounded-lg border border-gray-200 dark:border-gray-800 w-full">
              <div className="grid gap-2 p-4">
                <h2 className="text-lg font-medium">drop a file here</h2>
                <div className="flex items-center justify-center ">
  <img src="https://cdn-icons-png.flaticon.com/512/5868/5868106.png" alt="Drag and Drop" className="h-15 w-14 mb-1 align-middle" />
</div>
<div className="flex flex-col h-[200px]">
                  <input
                    type="file"
                    className="hidden"
                    id="file-upload"
                    onChange={handleFileChange}
                  />
                  <label
                    htmlFor="file-upload"
                    className="cursor-pointer text-blue-600 dark:text-blue-400"
                  >
                    {filename}
                  </label>
                </div>
              </div>
            </div>
            {/* Subheadings section */}
            <div className="rounded-lg border border-gray-200 dark:border-gray-800 w-full">
              <div className="grid gap-2 p-4">
                <h2 className="text-lg font-medium">Subheadings</h2>
                <div className="flex flex-col items-center justify-center h-[150px] border border-dashed border-gray-300 dark:border-gray-700 rounded-lg overflow-y-auto">
                  <Subheadings selectedFile={selectedFile} />
                </div>
              </div>
            </div>
          </div>
          <div className='flex justify-center'>
            <button onClick={handleFileUpload} className='bg-black text-white py-2  w-[100px]'>
              {loading ? 'Summarizing...' : 'Summarise'}
            </button>
            {loading && <div className="loader"></div>}
          </div>
          {/* Preferences section */}
          <div className="rounded-lg border border-gray-200 dark:border-gray-800 w-full">
            <div className="grid gap-2 p-4">
              <h2 className="text-lg font-medium">Preferences</h2>
              <div className="flex flex-col h-[50px]">
                <input
                  className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 flex-1 resize-none"
                  placeholder="Enter your preferences here."
                  type="text"
                  style={{ height: '50px' }}
                />
              </div>
            </div>
          </div>
          {/* Output section */}
          <div className="rounded-lg border border-gray-200 dark:border-gray-800 w-full">
            <div className="grid gap-2 p-4">
              <h2 className="text-lg font-medium">SUMMARY</h2>
              <div className="flex flex-col h-[300px]">
                <textarea
                  className="flex min-h-[80px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50 flex-1 resize-none"
                  placeholder="Your summarized text will appear here."
                  readOnly
                  value={summaries} // Display summaries data in textarea
                ></textarea>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default Home;
