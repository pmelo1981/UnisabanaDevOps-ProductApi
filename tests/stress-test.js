import http from 'k6/http';
import { check, sleep } from 'k6';
import { randomString } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';

// Base URL of the deployed API
const BASE_URL = __ENV.TARGET_URL || 'http://productapi-mpn.centralus.cloudapp.azure.com';

export const options = {
  stages: [
    { duration: '1m', target: 50 },  // Ramp-up to 50 users
    { duration: '3m', target: 100 }, // Stress phase at 100 users
    { duration: '1m', target: 0 },   // Ramp-down to 0 users
  ],
  thresholds: {
    http_req_failed: ['rate<0.01'],  // Error rate must be less than 1%
    http_req_duration: ['p(95)<1000'], // 95% of requests must complete below 1000ms (1s)
  },
};

export default function () {
  // 1. Get all products (Weight: 45%)
  let resAll = http.get(`${BASE_URL}/api/products`);
  check(resAll, {
    'get all status is 200': (r) => r.status === 200,
  });
  sleep(0.5);

  // 2. Get statistics (Weight: 20%)
  let resStats = http.get(`${BASE_URL}/api/products/stats`);
  check(resStats, {
    'get stats status is 200': (r) => r.status === 200,
  });
  sleep(0.5);

  // 3. Create a new product (Weight: 15%)
  let payload = JSON.stringify({
    name: `Product_${randomString(5)}`,
    description: 'Generated during k6 stress test',
    price: parseFloat((Math.random() * 500).toFixed(2)),
    stock: Math.floor(Math.random() * 100) + 1
  });

  let params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  let resCreate = http.post(`${BASE_URL}/api/products`, payload, params);
  let createdId = null;
  check(resCreate, {
    'create status is 201': (r) => r.status === 201,
  });
  
  if (resCreate.status === 201) {
    createdId = JSON.parse(resCreate.body).id;
  }
  sleep(0.5);

  // 4. Retrieve the newly created product by ID (Weight: 15%)
  if (createdId) {
    let resById = http.get(`${BASE_URL}/api/products/${createdId}`);
    check(resById, {
      'get by id status is 200': (r) => r.status === 200,
    });
    sleep(0.5);

    // 5. Delete the product to avoid unlimited memory growth in-memory (Weight: 5%)
    // (This also checks the delete throughput and reduces memory footprint)
    if (Math.random() < 0.3) {
      let resDelete = http.del(`${BASE_URL}/api/products/${createdId}`);
      check(resDelete, {
        'delete status is 204': (r) => r.status === 204,
      });
      sleep(0.5);
    }
  }
}
