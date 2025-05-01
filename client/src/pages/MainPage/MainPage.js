import React, { useState, useEffect } from 'react';
import { Container, Row, Col, Alert } from 'react-bootstrap';
import Navigation from '../../components/layout/Navigation/Navigation';
import UserDashboard from '../../components/layout/UserDashboard/UserDashboard';
import { jwtDecode } from 'jwt-decode';
import './MainPage.css';

const MainPage = () => {
  const [currentUser, setCurrentUser] = useState(null);
  const [error, setError] = useState('');

  useEffect(() => {
    const checkAuth = () => {
      const token = localStorage.getItem('userToken');
      const userData = localStorage.getItem('currentUser');
      
      if (token && userData) {
        try {
          const decoded = jwtDecode(token);
          const user = JSON.parse(userData);
          
          if (decoded.nameid === user.id && decoded.exp * 1000 > Date.now()) {
            setCurrentUser(user);
          } else {
            handleLogout();
          }
        } catch (err) {
          console.error('Invalid token or user data:', err);
          handleLogout();
        }
      }
    };

    checkAuth();

    const handleStorageChange = () => {
      checkAuth();
    };

    window.addEventListener('storage', handleStorageChange);
    return () => window.removeEventListener('storage', handleStorageChange);
  }, []);

  const handleLogout = () => {
    localStorage.removeItem('userToken');
    localStorage.removeItem('currentUser');
    setCurrentUser(null);
    setError('');
  };

  return (
    <div className="main-page">
      <Navigation />
      
      <Container className="main-container">
        <Row className="justify-content-center">
          <Col md={8} className="text-center">
            {error && (
              <Alert 
                variant="danger" 
                dismissible 
                onClose={() => setError('')}
                className="mb-4"
              >
                {error}
              </Alert>
            )}

            {!currentUser ? (
              <div className="welcome-section">
                <h1 className="main-title">Добро пожаловать!</h1>
                <p className="main-subtitle">
                  Для доступа к функциям системы войдите в свой аккаунт
                </p>
              </div>
            ) : (
              <UserDashboard user={currentUser} onLogout={handleLogout} />
            )}
          </Col>
        </Row>
      </Container>
    </div>
  );
};

export default MainPage;