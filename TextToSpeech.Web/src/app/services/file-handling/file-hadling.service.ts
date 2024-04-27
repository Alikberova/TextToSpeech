import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class FileHadlingService {

  constructor(private http: HttpClient) {
    // http.get<object[]>('/').subscribe(result => {
    //   this.forecasts = result;
    // }, error => console.error(error));
  }

  
  onFileSelected(event: any): void {
    if (!event.target || !event.target.files) {
      return;
    }
    const file = event.target.files[0];
    if (file) {
      // You can perform additional checks or operations with the selected file here
      console.log('Selected File:', file);
    }
  }

  uploadBook(): void {
    const fileInput = document.getElementById('fileInput') as HTMLInputElement;
    const file = fileInput?.files?.[0];

    if (file) {
      const formData = new FormData();
      formData.append('file', file);

      // Replace 'YOUR_BACKEND_API_ENDPOINT' with your actual backend API endpoint for file upload
      const apiUrl = 'YOUR_BACKEND_API_ENDPOINT';

      this.http.post(apiUrl, formData).subscribe(
        (response) => {
          console.log('Upload successful!', response);
          // You can handle the response from the backend here
        },
        (error) => {
          console.error('Error uploading file:', error);
          // You can handle errors here
        }
      );
    } else {
      console.error('No file selected.');
      // You can display an error message or take appropriate action
    }
  }
}
