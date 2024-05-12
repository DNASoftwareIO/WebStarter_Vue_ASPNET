const request = require('supertest');

function createRandomString(length) {
  const chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
  let result = "";
  for (let i = 0; i < length; i++) {
    result += chars.charAt(Math.floor(Math.random() * chars.length));
  }
  return result;
}

describe('User Api', () => {
  const baseUrl = 'http://localhost:5286'; // TODO get from config/env vars
  var userName = createRandomString(12);
  var email = userName.toLowerCase() + "@test.com";
  var validRefreshToken = '';

  describe('/register', () => {
    it('Should return 200', async () => {
      const res = await request(baseUrl).post('/user/register')
                        .send({userName, password: '12345678', email});

      expect(res.statusCode).toBe(200);
      expect(res.body).toHaveProperty('jwt');
      expect(res.body).toHaveProperty('refreshToken');
      expect(res.body).toHaveProperty('sessionId');
      expect(res.body).toHaveProperty('user');
      expect(res.body.user.userName).toBe(userName);
      expect(res.body.user.email).toBe(email);

      validRefreshToken = res.body.refreshToken;
    });

    it('Should return 400 if username exists', async () => {
      const res = await request(baseUrl).post('/user/register')
                        .set('Accept', 'application/json')
                        .send({userName, password: '12345678', email: 'different@email.com'});
      
      expect(res.statusCode).toBe(400);
      expect(res.body).toBe('Error registering account. Please try again later.');
    });

    it('Should return 400 if email exists', async () => {
      const res = await request(baseUrl).post('/user/register')
                        .set('Accept', 'application/json')
                        .send({userName: 'differentUser', password: '12345678', email: email});
      
      expect(res.statusCode).toBe(400);
      expect(res.body).toBe('Error registering account. Please try again later.');
    });

    it('Should return 400 if username empty', async () => {
      const res = await request(baseUrl).post('/user/register')
                        .set('Accept', 'application/json')
                        .send({password: '12345678', email});
      
                        expect(res.statusCode).toBe(400);
      expect(res.body.errors).toStrictEqual({"UserName": ["The UserName field is required."]});
    });

    it('Should return 400 if email empty', async () => {
      const res = await request(baseUrl).post('/user/register')
                        .set('Accept', 'application/json')
                        .send({userName, password: '12345678'});
      
      expect(res.statusCode).toBe(400);
      expect(res.body.errors).toStrictEqual({"Email": ["The Email field is required."]});
    });

    it('Should return 400 if password empty', async () => {
      const res = await request(baseUrl).post('/user/register')
                        .set('Accept', 'application/json')
                        .send({userName, email});
      
      expect(res.statusCode).toBe(400);
      expect(res.body.errors).toStrictEqual({"Password": ["The Password field is required."]});
    });
  });

  describe('/login', () => {
    it('Should return 200', async () => {
      const res = await request(baseUrl).post('/user/login')
                        .send({userName, password: '12345678'});

      expect(res.statusCode).toBe(200);
      expect(res.body).toHaveProperty('jwt');
      expect(res.body).toHaveProperty('refreshToken');
      expect(res.body).toHaveProperty('sessionId');
      expect(res.body).toHaveProperty('user');
      expect(res.body.user.userName).toBe(userName);
      expect(res.body.user.email).toBe(email);
    });

    it('Should return 400 if username does not exist', async () => {
      const res = await request(baseUrl).post('/user/login')
                        .set('Accept', 'application/json')
                        .send({userName: 'unknown', password: '12345678'});
      
      expect(res.statusCode).toBe(400);
      expect(res.body).toBe('Error logging in. Please try again later.');
    });

    it('Should return 400 if password wrong', async () => {
      const res = await request(baseUrl).post('/user/login')
                        .set('Accept', 'application/json')
                        .send({userName, password: 'wrong'});
      
      expect(res.statusCode).toBe(400);
      expect(res.body).toBe('Error logging in. Please try again later.');
    });

    // TODO add test for when user is deleted
    // TODO add tests for tfa enabled etc

    it('Should return 400 if username empty', async () => {
      const res = await request(baseUrl).post('/user/login')
                        .set('Accept', 'application/json')
                        .send({password: '12345678'});
      
                        expect(res.statusCode).toBe(400);
      expect(res.body.errors).toStrictEqual({"UserName": ["The UserName field is required."]});
    });

    it('Should return 400 if password empty', async () => {
      const res = await request(baseUrl).post('/user/login')
                        .set('Accept', 'application/json')
                        .send({userName});
      
      expect(res.statusCode).toBe(400);
      expect(res.body.errors).toStrictEqual({"Password": ["The Password field is required."]});
    });
  });

  describe('/refresh-token', () => {
    it('Should return 200', async () => {
      const res = await request(baseUrl).post('/user/refresh-token')
                        .send({refreshToken: validRefreshToken});

      expect(res.statusCode).toBe(200);
      expect(res.body).toHaveProperty('jwt');
    });

    it('Should return 400 if invalid token', async () => {
      const res = await request(baseUrl).post('/user/refresh-token')
                        .set('Accept', 'application/json')
                        .send({refreshToken: '0a6ca2f0-08f0-4d20-9e9f-d5511f3a1917'});
      
      expect(res.statusCode).toBe(400);
      expect(res.body).toBe('Session not found or expired.');
    });

    // TODO no token passed, session expired, user deleted, 
  });

});