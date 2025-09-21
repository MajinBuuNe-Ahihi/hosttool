// src/utils/http.ts
import axios from 'axios';

const http = axios.create({
  baseURL: 'https://hosttool.onrender.com/api', // baseURL theo backend của bạn
  timeout: 10000,
});

// Interceptor request
http.interceptors.request.use(
  (config) => {
    // Thêm token nếu có
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Interceptor response
http.interceptors.response.use(
  (response) => response.data,
  (error) => {
    console.error('API Error:', error);
    return Promise.reject(error);
  }
);

export default http;
